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

        //Texture2DArray diffMasksA = new Texture2DArray(layerA.width, layerA.height, framesPerArray, TextureFormat.R8, false, true);
        //Texture2DArray diffMasksB = new Texture2DArray(layerB.width, layerB.height, framesPerArray, TextureFormat.R8, false, true);

        int index = 0;

        // 🔹 Layer A
        for (int i = 0; i < framesPerArray - 1; i++)
        {
            Texture2D tex1 = Extract(layerA, i);
            Texture2D tex2 = Extract(layerA, i + 1);

            //Texture2D mask = BuildDiffMask(tex1, tex2);

            //Graphics.CopyTexture(mask, 0, 0, diffMasksA, i, 0);

            data.diffs[index++] = ComputeDiff(tex1, tex2);

        }

        // 🔹 Transição A → B
        Texture2D texture1 = Extract(layerA, framesPerArray - 1);
        Texture2D texture2 = Extract(layerB, 0);

        //Texture2D diffMask = BuildDiffMask(texture1, texture2);

        //Graphics.CopyTexture(diffMask, 0, 0, diffMasksA, framesPerArray - 1, 0);

        data.diffs[index++] = ComputeDiff(texture1, texture2);

        // 🔹 Layer B
        for (int i = 0; i < framesPerArray - 1; i++)
        {
            Texture2D tex1 = Extract(layerB, i);
            Texture2D tex2 = Extract(layerB, i + 1);

           // Texture2D mask = BuildDiffMask(tex1, tex2);

            //Graphics.CopyTexture(mask, 0, 0, diffMasksB, i, 0);

            data.diffs[index++] = ComputeDiff(tex1, tex2);
        }

        // 4️⃣ Salvar asset
        AssetDatabase.CreateAsset(
            data,
            "Assets/FrameDiffData.asset"
        );

        /*AssetDatabase.CreateAsset(
            diffMasksA,
            "Assets/diffMasksA.asset"
        );


        AssetDatabase.CreateAsset(
            diffMasksB,
            "Assets/diffMasksB.asset"
        );*/

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Pronto",
            "FrameDiffData.asset, Assets/diffMasksA e diffMasksB.asset foram gerados com sucesso",
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

    /*public static Texture2D BuildDiffMask( Texture2D a, Texture2D b, float pixelThreshold = 0.05f)
    {
        int w = a.width;
        int h = a.height;

        Texture2D mask = new Texture2D(
            w, h,
            TextureFormat.R8,
            false,
            true // linear
        );

        var pa = a.GetPixels32();
        var pb = b.GetPixels32();
        var outPixels = new Color32[pa.Length];

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
    }*/
}
