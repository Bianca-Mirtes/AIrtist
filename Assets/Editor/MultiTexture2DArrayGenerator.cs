using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class MultiTexture2DArrayGenerator : EditorWindow
{
    string folderPath = "Assets/Starry_Night/1";
    string outputFolder = "Assets/PaintingArrays";
    int batchSize = 200; // 100 frames por array

    [MenuItem("Tools/Create Multiple Texture2DArray")]
    public static void OpenWindow()
    {
        GetWindow<MultiTexture2DArrayGenerator>("Multi Texture2DArray Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Texture2DArray (auto split)", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Frames Folder", folderPath);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        batchSize = EditorGUILayout.IntField("Batch Size", batchSize);

        if (GUILayout.Button("Generate Arrays"))
        {
            Generate();
        }
    }

    void Generate()
    {
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly)
            .Select(f => f.Replace("\\", "/"))
            .OrderBy(f => f, System.StringComparer.Ordinal) // ordenação correta
            .ToArray();

        if (files.Length == 0)
        {
            Debug.LogError("Nenhuma imagem encontrada!");
            return;
        }

        // Carrega a primeira textura para pegar dimensão e formato
        Texture2D first = AssetDatabase.LoadAssetAtPath<Texture2D>(files[0]);
        int width = first.width;
        int height = first.height;

        int totalFrames = files.Length;
        int arrayCount = Mathf.CeilToInt(totalFrames / (float)batchSize);

        Debug.Log($"Total frames: {totalFrames}, criando {arrayCount} arrays...");

        // Loop dos arrays
        for (int a = 0; a < arrayCount; a++)
        {
            int start = a * batchSize;
            int end = Mathf.Min(start + batchSize, totalFrames);
            int count = end - start;

            Debug.Log($"Criando array {a}: frames {start} até {end - 1}");

            Texture2DArray arr = new Texture2DArray(
                width,
                height,
                count,
                TextureFormat.RGBA32,
                false
            );

            arr.wrapMode = TextureWrapMode.Clamp;
            arr.filterMode = FilterMode.Bilinear;

            // Preenche este array
            for (int i = 0; i < count; i++)
            {
                string path = files[start + i];
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                // se a textura não tem o mesmo tamanho do primeiro, redimensiona
                if (tex.width != width || tex.height != height)
                {
                    Texture2D resized = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    // Bilinear scaling
                    RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(tex, rt);
                    RenderTexture prev = RenderTexture.active;
                    RenderTexture.active = rt;
                    resized.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    resized.Apply();
                    RenderTexture.active = prev;
                    RenderTexture.ReleaseTemporary(rt);

                    // set pixels e destrói tex temporária
                    arr.SetPixels(resized.GetPixels(), i);
                    DestroyImmediate(resized);
                }
                else
                {
                    arr.SetPixels(tex.GetPixels(), i);
                }

                Resources.UnloadAsset(tex);

                // libera a textura carregada
                DestroyImmediate(tex);
            }

            arr.Apply();

            string assetPath = $"{outputFolder}/PaintingArray_{a}.asset";
            AssetDatabase.CreateAsset(arr, assetPath);

            Debug.Log($"Criado: {assetPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✔ Todos os Texture2DArray foram gerados com sucesso!");
    }
}