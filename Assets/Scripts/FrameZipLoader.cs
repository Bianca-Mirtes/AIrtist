using GLTFast.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Rendering;
using WorkData;

public class FrameZipLoader : MonoBehaviour
{
    [Header("Config")]
    public int framesPerArray = 200;
    public int frameWidth = 750;
    public int frameHeight = 598;
    public TextureFormat format = TextureFormat.ARGB32;

    private Texture2DArray paintingA;
    private Texture2DArray paintingB;
    private Texture2DArray maskA;
    private Texture2DArray maskB;

    private Sprite workImage;
    public UnityEngine.Material artMat;

    string workDir;
    private string zipPath;
    public string zipFileName = "analysis_out5.zip";
    public TextAsset txtFile;

    private void Start()
    {
        Debug.Log(Path.Combine(Application.streamingAssetsPath, zipFileName));
        LoadFromPath(Path.Combine(Application.streamingAssetsPath, zipFileName), txtFile.text);
    }

    /// <summary>
    /// Entrada principal: recebe ZIP em bytes da API
    /// </summary>
    public void LoadFromZipBytes(byte[] zipBytes, string txtFile)
    {
        StartCoroutine(LoadRoutine(zipBytes, txtFile));
    }

    public void LoadFromPath(string path, string txtFile)
    {
        StartCoroutine(Load(path, txtFile));
    }

    IEnumerator Load(string zipPAth, string txt)
    {
        // 3️⃣ Cria arrays vazios
        paintingA = CreateArray();
        paintingB = CreateArray();
        maskA = CreateArray();
        maskB = CreateArray();

        workDir = Path.Combine(Application.persistentDataPath, "painting_zip_tmp");

        if (Directory.Exists(workDir))
            Directory.Delete(workDir, true);

        Directory.CreateDirectory(workDir);

        // 4️⃣ Coleta entradas do ZIP
        List<ZipArchiveEntry> paintingEntries = new();
        List<ZipArchiveEntry> maskEntries = new();

        using (ZipArchive archive = ZipFile.OpenRead(zipPAth))
        {
            foreach (var entry in archive.Entries)
            {
                string path = entry.FullName.Replace("\\", "/");

                if (path.Contains("/painting/") && !entry.FullName.EndsWith("/"))
                    paintingEntries.Add(entry);

                else if (path.Contains("/mask/") && !entry.FullName.EndsWith("/"))
                    maskEntries.Add(entry);
            }

            // 5️⃣ Ordenação garantida por nome
            paintingEntries.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            maskEntries.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            if ((paintingEntries.Count-1) != maskEntries.Count)
            {
                Debug.LogError("❌ Quantidade de frames painting e mask não bate!");
                yield break;
            }

            int totalFrames = Mathf.Min(paintingEntries.Count-1, framesPerArray * 2);

            // 6️⃣ Loop sincronizado
            for (int ii = 0; ii < totalFrames; ii++)
            {
                // --- Painting ---
                Texture2D paintTex = ExtractAndLoad(paintingEntries[ii]);
                UploadFrame(paintTex, paintingA, paintingB, ii);

                // --- Mask ---
                Texture2D maskTex = ExtractAndLoad(maskEntries[ii]);
                UploadFrame(maskTex, maskA, maskB, ii);

                if (ii == totalFrames)
                {
                    workImage = Sprite.Create(
                        paintTex, // Texture2D
                        new Rect(0, 0, paintTex.width, paintTex.height), // área
                        new Vector2(0.5f, 0.5f),          // pivot (centro)
                        100f                              // pixels per unit
                    );
                }

                Destroy(paintTex);
                Destroy(maskTex);

                if (ii % 2 == 0)
                    yield return null;
            }
        }

        // 7️⃣ Finaliza
        paintingA.Apply(false, true);
        paintingB.Apply(false, true);
        maskA.Apply(false, true);
        maskB.Apply(false, true);

        Infos infos = TXTLoader.Instance.LoadTXT(txt);

        ChooseArtController.Instance.CreateNewArt(
            paintingA,
            paintingB,
            maskA,
            maskB,
            "Usuário",
            "Minha Obra",
            DateTime.Now.Year.ToString(),
            workImage
        );

        artMat.SetTexture("_LayerA", paintingA);
        artMat.SetTexture("_LayerB", paintingB);
        artMat.SetTexture("_LayerA_DiffMasks", maskA);
        artMat.SetTexture("_LayerB_DiffMasks", maskB);

        Debug.Log("✅ Painting + Masks carregados com sucesso!");
    }
    IEnumerator LoadRoutine(byte[] zipBytes, string txt)
    {
        // 1️⃣ Workspace
        workDir = Path.Combine(Application.persistentDataPath, "painting_zip_tmp");
        zipPath = Path.Combine(workDir, "frames.zip");

        if (Directory.Exists(workDir))
            Directory.Delete(workDir, true);

        Directory.CreateDirectory(workDir);

        // 2️⃣ Salva ZIP
        File.WriteAllBytes(zipPath, zipBytes);
        yield return null;

        // 3️⃣ Cria arrays vazios
        paintingA = CreateArray();
        paintingB = CreateArray();
        maskA = CreateArray();
        maskB = CreateArray();

        // 4️⃣ Coleta entradas do ZIP
        List<ZipArchiveEntry> paintingEntries = new();
        List<ZipArchiveEntry> maskEntries = new();

        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (var entry in archive.Entries)
            {
                string path = entry.FullName.Replace("\\", "/");

                if (path.Contains("/painting/") && !entry.FullName.EndsWith("/"))
                    paintingEntries.Add(entry);

                else if (path.Contains("/mask/") && !entry.FullName.EndsWith("/"))
                    maskEntries.Add(entry);
            }

            // 5️⃣ Ordenação garantida por nome
            paintingEntries.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            maskEntries.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            if (paintingEntries.Count-1 != maskEntries.Count)
            {
                Debug.LogError("❌ Quantidade de frames painting e mask não bate!");
                yield break;
            }

            int totalFrames = Mathf.Min(paintingEntries.Count-1, framesPerArray * 2);

            // 6️⃣ Loop sincronizado
            for (int ii = 0; ii < totalFrames; ii++)
            {
                // --- Painting ---
                Texture2D paintTex = ExtractAndLoad(paintingEntries[ii]);
                UploadFrame(paintTex, paintingA, paintingB, ii);

                // --- Mask ---
                Texture2D maskTex = ExtractAndLoad(maskEntries[ii]);
                UploadFrame(maskTex, maskA, maskB, ii);

                if(ii == totalFrames)
                {
                    workImage = Sprite.Create(
                        paintTex, // Texture2D
                        new Rect(0, 0, paintTex.width, paintTex.height), // área
                        new Vector2(0.5f, 0.5f),          // pivot (centro)
                        100f                              // pixels per unit
                    );
                }

                Destroy(paintTex);
                Destroy(maskTex);

                // estrategia de throttling
                if (ii % 2 == 0) // para distribuir o custo ao longo do tempo (a cada dois frames processados espera um frame) 
                    yield return null;
            }
        }

        // 7️⃣ Finaliza
        paintingA.Apply(false, true);
        paintingB.Apply(false, true);
        maskA.Apply(false, true);
        maskB.Apply(false, true);

        Infos infos = TXTLoader.Instance.LoadTXT(txt);

        ChooseArtController.Instance.CreateNewArt(
            paintingA,
            paintingB,
            maskA,
            maskB,
            infos.authorName,
            infos.workName,
            infos.workAge,
            workImage
        );

        Debug.Log("✅ Painting + Masks carregados com sucesso!");
    }

    Texture2DArray CreateArray()
    {
        Texture2DArray arr = new Texture2DArray(
            frameWidth,
            frameHeight,
            framesPerArray,
            format,
            false
        );

        arr.wrapMode = TextureWrapMode.Clamp;
        arr.filterMode = FilterMode.Bilinear;
        return arr;
    }

    Texture2D ExtractAndLoad(ZipArchiveEntry entry)
    {
        string path = Path.Combine(workDir, entry.Name);
        entry.ExtractToFile(path, true);

        byte[] data = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2, format, false);

        tex.LoadImage(data, markNonReadable: true);

        File.Delete(path);
        return tex;
    }

    void UploadFrame(Texture2D tex, Texture2DArray arrayA, Texture2DArray arrayB, int index)
    {
        Texture2DArray target = (index < framesPerArray) ? arrayA : arrayB;
        int slice = index % framesPerArray;

        // Caminho rápido (GPU → GPU)
        if (SystemInfo.copyTextureSupport != CopyTextureSupport.None &&
            tex.format == target.format)
        {
            Graphics.CopyTexture(tex, 0, 0, target, slice, 0);
        }
        else
        {
            // Fallback seguro (Editor)
            RenderTexture rt = RenderTexture.GetTemporary(
                frameWidth,
                frameHeight,
                0,
                RenderTextureFormat.ARGB32
            );

            Graphics.Blit(tex, rt);
            Graphics.CopyTexture(rt, 0, 0, target, slice, 0);
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}

