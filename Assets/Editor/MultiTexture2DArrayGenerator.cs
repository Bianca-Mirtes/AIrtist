using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class MultiTexture2DArrayGenerator : EditorWindow
{
    string folderPath = "Assets/Sprites/Painting1";
    string outputFolder = "Assets/PaintingArrays";
    int batchSize = 100; // 100 frames por array

    [MenuItem("Tools/Create Multiple Texture2DArray")]
    public static void OpenWindow()
    {
        GetWindow<MultiTexture2DArrayGenerator>("Multi Texture2DArray Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate 5 Texture2DArray (auto split)", EditorStyles.boldLabel);

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

                // Forçar leitura e formato correto
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }

                // Copia a textura sem estourar memória
                Texture2D readable = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                Graphics.CopyTexture(tex, readable);

                arr.SetPixels(readable.GetPixels(), i);
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