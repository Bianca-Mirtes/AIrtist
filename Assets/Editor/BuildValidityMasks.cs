using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class BuildValidityMasks
{
    [MenuItem("Tools/Build Validity Masks From Folder")]
    static void Build()
    {
        string folder = "Assets/YourOverlayFolder"; // ajuste
        string[] files = Directory.GetFiles(folder, "*.png").OrderBy(f => f).ToArray();

        if (files.Length == 0) { Debug.LogError("Nenhum arquivo"); return; }

        Texture2D first = AssetDatabase.LoadAssetAtPath<Texture2D>(files[0].Replace(Application.dataPath, "Assets"));
        int w = first.width, h = first.height;
        int count = files.Length;

        Texture2DArray arr = new Texture2DArray(w, h, count, TextureFormat.R8, false, true);
        arr.filterMode = FilterMode.Point;
        arr.wrapMode = TextureWrapMode.Clamp;

        List<int> totalValidPerFrame = new List<int>(count);

        Texture2D prev = null;
        for (int i = 0; i < count; i++)
        {
            string assetPath = files[i].Replace(Application.dataPath, "Assets");
            Texture2D cur = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (cur == null) { Debug.LogError("Não carregou: " + assetPath); return; }

            // Force readable import settings (se necessário)
            string impPath = assetPath;
            var ti = AssetImporter.GetAtPath(impPath) as TextureImporter;
            if (ti != null)
            {
                ti.isReadable = true;
                ti.sRGBTexture = false; // abordamos em linear aqui
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.filterMode = FilterMode.Point;
                AssetDatabase.ImportAsset(impPath, ImportAssetOptions.ForceUpdate);
                cur = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath); // recarregar
            }

            Color32[] pixels = cur.GetPixels32();

            // se prev == null → validade é aquilo que difere do fundo (por exemplo alpha)
            Color32[] prevPixels = prev != null ? prev.GetPixels32() : null;

            byte[] layerRaw = new byte[w * h];
            int validCount = 0;

            for (int p = 0; p < pixels.Length; p++)
            {
                bool isNew = false;

                if (prevPixels == null)
                {
                    // primeira frame: decidir por alpha ou cor diferente do "background"
                    isNew = pixels[p].a > 10; // ajustar threshold
                }
                else
                {
                    // se pixel atual difere do anterior -> é novo traço (differential)
                    // comparar por alfa ou cor (ajuste threshold se necessário)
                    bool curPaint = pixels[p].a > 10;
                    bool prevPaint = prevPixels[p].a > 10;
                    isNew = curPaint && !prevPaint;
                }

                if (isNew)
                {
                    layerRaw[p] = 255;
                    validCount++;
                }
                else
                {
                    layerRaw[p] = 0;
                }
            }

            // Carrega dados brutos na Texture2D temporária e copie para o Texture2DArray
            Texture2D tmp = new Texture2D(w, h, TextureFormat.R8, false, true);
            tmp.filterMode = FilterMode.Point;
            tmp.wrapMode = TextureWrapMode.Clamp;
            tmp.LoadRawTextureData(layerRaw);
            tmp.Apply(false, true);

            // Copia para array
            Graphics.CopyTexture(tmp, 0, 0, arr, i, 0);

            totalValidPerFrame.Add(validCount);
            prev = cur;
        }

        // Salva o Texture2DArray como asset
        string outPath = "Assets/Generated/ValidityFrames.asset";
        if (!Directory.Exists("Assets/Generated")) Directory.CreateDirectory("Assets/Generated");
        AssetDatabase.CreateAsset(arr, outPath);

        // Salva os totais em ScriptableObject simples
        var meta = ScriptableObject.CreateInstance<ValidityFramesMeta>();
        meta.totalValidPerFrame = totalValidPerFrame.ToArray();
        AssetDatabase.CreateAsset(meta, "Assets/Generated/ValidityFramesMeta.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Criadas {count} masks, saved to {outPath}");
    }
}

public class ValidityFramesMeta : ScriptableObject
{
    public int[] totalValidPerFrame;
}
