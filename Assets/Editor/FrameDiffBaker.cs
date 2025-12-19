using UnityEngine;
using UnityEditor;

public static class FrameDiffBaker
{
    [MenuItem("Scratch/Bake Frame Diffs")]
    public static void Bake()
    {
        // 1️⃣ Selecionar Layer A
        var layerA = Selection.activeObject as Texture2DArray;
        if (layerA == null)
        {
            EditorUtility.DisplayDialog(
                "Erro",
                "Selecione o Texture2DArray da Layer A no Project Window",
                "OK"
            );
            return;
        }

        // 2️⃣ Selecionar Layer B
        string path = EditorUtility.OpenFilePanel(
            "Selecione o Texture2DArray da Layer B",
            "Assets",
            "asset"
        );

        if (string.IsNullOrEmpty(path))
            return;

        path = path.Substring(path.IndexOf("Assets"));
        var layerB = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);

        int framesPerArray = layerA.depth;

        // 3️⃣ Criar asset de saída
        var data = ScriptableObject.CreateInstance<FrameDiffData>();
        data.framesPerArray = framesPerArray;
        data.diffs = new float[framesPerArray * 2 - 1];

        int index = 0;

        // 🔹 Layer A
        for (int i = 0; i < framesPerArray - 1; i++)
        {
            data.diffs[index++] =
                ComputeDiff(
                    Extract(layerA, i),
                    Extract(layerA, i + 1)
                );
        }

        // 🔹 Transição A → B
        data.diffs[index++] =
            ComputeDiff(
                Extract(layerA, framesPerArray - 1),
                Extract(layerB, 0)
            );

        // 🔹 Layer B
        for (int i = 0; i < framesPerArray - 1; i++)
        {
            data.diffs[index++] =
                ComputeDiff(
                    Extract(layerB, i),
                    Extract(layerB, i + 1)
                );
        }

        // 4️⃣ Salvar asset
        AssetDatabase.CreateAsset(
            data,
            "Assets/FrameDiffData.asset"
        );

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Pronto",
            "FrameDiffData.asset foi gerado com sucesso",
            "OK"
        );
    }

    static Texture2D Extract(Texture2DArray src, int slice)
    {
        Texture2D tex = new Texture2D(
            src.width,
            src.height,
            TextureFormat.RGBA32,
            false,
            false
        );

        Graphics.CopyTexture(src, slice, 0, tex, 0, 0);
        return tex;
    }

    static float ComputeDiff(Texture2D a, Texture2D b)
    {
        var pa = a.GetPixels32();
        var pb = b.GetPixels32();

        int diff = 0;

        for (int i = 0; i < pa.Length; i++)
        {
            float d =
                (Mathf.Abs(pa[i].r - pb[i].r) +
                 Mathf.Abs(pa[i].g - pb[i].g) +
                 Mathf.Abs(pa[i].b - pb[i].b)) / (3f * 255f);

            if (d > 0.05f)
                diff++;
        }

        Object.DestroyImmediate(a);
        Object.DestroyImmediate(b);

        return diff / (float)pa.Length;
    }
}
