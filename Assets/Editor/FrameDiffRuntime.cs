using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class FrameDiffBakerPNG
{
    [MenuItem("Scratch/Bake Frame Diff Masks (PNG)")]
    public static void Bake()
    {
        // 1️⃣ Pasta de entrada
        string inputFolder = EditorUtility.OpenFolderPanel(
            "Selecione a pasta com TODOS os frames PNG",
            "Assets",
            ""
        );

        if (string.IsNullOrEmpty(inputFolder))
            return;

        // 2️⃣ Pasta de saída
        string outputFolder = EditorUtility.OpenFolderPanel(
            "Selecione a pasta de SAÍDA das máscaras",
            "Assets",
            ""
        );

        if (string.IsNullOrEmpty(outputFolder))
            return;

        // Converte paths absolutos → Assets/
        inputFolder = ToProjectPath(inputFolder);
        outputFolder = ToProjectPath(outputFolder);

        // 3️⃣ Coleta frames
        string[] files = Directory
            .GetFiles(inputFolder, "*.png", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, System.StringComparer.Ordinal)
            .ToArray();

        if (files.Length < 2)
        {
            EditorUtility.DisplayDialog(
                "Erro",
                "É necessário pelo menos 2 frames PNG.",
                "OK"
            );
            return;
        }

        Debug.Log($"🔍 {files.Length} frames encontrados.");

        // 4️⃣ Loop de geração
        for (int i = 0; i < files.Length - 1; i++)
        {
            Texture2D a = LoadPNG(files[i]);
            Texture2D b = LoadPNG(files[i + 1]);

            Texture2D diff = BuildDiffMask(a, b);

            string outPath = Path.Combine(
                outputFolder,
                $"diff_{i:D3}.png"
            );

            File.WriteAllBytes(outPath, diff.EncodeToPNG());

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(diff);

            if (i % 10 == 0)
                EditorUtility.DisplayProgressBar(
                    "Baking Diff Masks",
                    $"Frame {i}/{files.Length - 1}",
                    i / (float)(files.Length - 1)
                );
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Pronto",
            "PNG das máscaras de diferença gerados com sucesso!",
            "OK"
        );
    }

    // =================== UTILS ===================

    static Texture2D LoadPNG(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
        tex.LoadImage(data, false);
        return tex;
    }

    static Texture2D BuildDiffMask(
        Texture2D a,
        Texture2D b,
        float pixelThreshold = 0.05f
    )
    {
        int w = a.width;
        int h = a.height;

        Texture2D mask = new Texture2D(
            w, h,
            TextureFormat.R8,
            false,
            true
        );

        Color32[] pa = a.GetPixels32();
        Color32[] pb = b.GetPixels32();
        Color32[] outPixels = new Color32[pa.Length];

        for (int i = 0; i < pa.Length; i++)
        {
            float d =
                (Mathf.Abs(pa[i].r - pb[i].r) +
                 Mathf.Abs(pa[i].g - pb[i].g) +
                 Mathf.Abs(pa[i].b - pb[i].b))
                / (3f * 255f);

            byte v = d > pixelThreshold ? (byte)255 : (byte)0;
            outPixels[i] = new Color32(v, v, v, 255);
        }

        mask.SetPixels32(outPixels);
        mask.Apply(false, false);
        return mask;
    }

    static string ToProjectPath(string absolutePath)
    {
        absolutePath = absolutePath.Replace("\\", "/");
        string projectPath = Application.dataPath.Replace("/Assets", "");
        return absolutePath.Replace(projectPath + "/", "");
    }
}
