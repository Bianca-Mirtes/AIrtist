using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
public class CreateTexture2DArray : EditorWindow
{
    string folderPath = "Assets/Starry_Night/1"; // pasta onde estão os PNGs
    string outputPath = "Assets/PaintingFrames.asset";
    int forceWidth = 0;   // opcional: força largura (0 = usa do primeiro)
    int forceHeight = 0;  // opcional: força altura (0 = usa do primeiro)

    [MenuItem("Tools/Create Texture2DArray")]
    public static void OpenWindow()
    {
        GetWindow<CreateTexture2DArray>("Create Texture2DArray");
    }

    private void OnGUI()
    {
        GUILayout.Label("Texture2DArray Generator (Safe)", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Frames Folder", folderPath);
        outputPath = EditorGUILayout.TextField("Output Asset Path", outputPath);
        forceWidth = EditorGUILayout.IntField("Force Width (0 = auto)", forceWidth);
        forceHeight = EditorGUILayout.IntField("Force Height (0 = auto)", forceHeight);

        if (GUILayout.Button("Generate Texture2DArray"))
        {
            Generate();
        }
    }

    void Generate()
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Folder não existe: " + folderPath);
            return;
        }

        // pega todos os PNGs ordenados por nome (ORDINAL)
        string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(".meta"))
            .OrderBy(f => f, System.StringComparer.Ordinal)
            .ToArray();

        if (files.Length == 0)
        {
            Debug.LogError("Nenhuma imagem encontrada!");
            return;
        }

        // lê primeiro arquivo com LoadImage para pegar dimensão real
        byte[] firstBytes = File.ReadAllBytes(files[0]);
        Texture2D firstTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        firstTex.LoadImage(firstBytes);

        int width = (forceWidth > 0) ? forceWidth : firstTex.width;
        int height = (forceHeight > 0) ? forceHeight : firstTex.height;
        int count = files.Length;

        Debug.Log($"Criando Texture2DArray: {count} frames — {width}x{height}");

        // Cria array em RGBA32 (seguro)
        Texture2DArray textureArray = new Texture2DArray(width, height, count, TextureFormat.RGBA32, false);
        textureArray.wrapMode = TextureWrapMode.Clamp;
        textureArray.filterMode = FilterMode.Bilinear;

        // Preenche camada por camada
        for (int i = 0; i < (count-100); i++)
        {
            string path = files[i];
            byte[] bytes = File.ReadAllBytes(path);

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool ok = tex.LoadImage(bytes, false); // false => não mark dynamic
            if (!ok)
            {
                Debug.LogError("Falha ao LoadImage: " + path);
                continue;
            }

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
                textureArray.SetPixels(resized.GetPixels(), i);
                DestroyImmediate(resized);
            }
            else
            {
                textureArray.SetPixels(tex.GetPixels(), i);
            }

            Resources.UnloadAsset(tex);

            // libera a textura carregada
            DestroyImmediate(tex);
        }

        textureArray.Apply();

        // salva asset
        string finalPath = outputPath;
        // garante que termina com .asset
        if (!finalPath.EndsWith(".asset")) finalPath = finalPath.TrimEnd('/') + ".asset";

        AssetDatabase.CreateAsset(textureArray, finalPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Texture2DArray gerado com sucesso! Total: " + count + " frames");
    }
}

