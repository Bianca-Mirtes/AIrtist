using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using WorkData;

public class FrameZipLoader : MonoBehaviour
{
    [Header("Config")]
    public TextureFormat format = TextureFormat.R8;

    Sprite workImage;

    ZipArchive archive;

    string workDir;

    // ================= ENTRY =================

    public void LoadFromZipPath(string zipPath, string txt, ArtWorkContext artWorkContext)
    {
        StartCoroutine(LoadRoutine(zipPath, txt, artWorkContext));
    }

    // ================= LOADER =================

    IEnumerator LoadRoutine(string zipPath, string txt, ArtWorkContext artWorkContext)
    {
        Debug.Log("📦 Iniciando extração do ZIP...");

        workDir = Path.Combine(Application.persistentDataPath, "painting_zip_tmp");

        if (Directory.Exists(workDir))
            Directory.Delete(workDir, true);

        Directory.CreateDirectory(workDir);

        archive = ZipFile.OpenRead(zipPath);

        List<ZipArchiveEntry> paintingEntries = new();
        List<ZipArchiveEntry> maskEntries = new();

        foreach (var entry in archive.Entries)
        {
            string path = entry.FullName.Replace("\\", "/").ToLower();

            if (!path.EndsWith("/"))
            {
                if (path.Contains("painting"))
                    paintingEntries.Add(entry);
                else if (path.Contains("mask"))
                    maskEntries.Add(entry);
            }
        }

        Debug.Log($"✅ Extração concluída! Encontradas {paintingEntries.Count} pinturas e {maskEntries.Count} máscaras.");

        paintingEntries.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        maskEntries.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        if (paintingEntries.Count != maskEntries.Count)
        {
            Debug.LogError("❌ Painting e Mask com quantidades diferentes!");
            yield break;
        }

        // --- cria buffers ---
        Texture2D paintingCurrent = LoadFrame(paintingEntries[0]);
        Texture2D maskCurrent = LoadFrame(maskEntries[0]);

        Texture2D image = LoadFrame(paintingEntries[paintingEntries.Count-1]);

        Debug.Log("🖼 Criando sprite da obra...");

        workImage = Sprite.Create(
            image,
            new Rect(0, 0, image.width, image.height),
            new Vector2(0.5f, 0.5f),
            100
        );

        Infos infos = TXTLoader.Instance.LoadTXT(txt);

        Debug.Log($"📝 Infos carregadas: Autor='{infos.authorName}', Título='{infos.workName}', Ano='{infos.workAge}', Dimensions:{infos.resHeight}x{infos.resWidth}");

        Debug.Log("🎨 Criando nova obra no ChooseArtController...");

        Debug.Log("ArtworkContext: " + artWorkContext.genre + " " + artWorkContext.title + " " + artWorkContext.sourceUrl + " " + artWorkContext.artistName + " " + artWorkContext.dimensions);

        ChooseArtController.Instance.CreateNewArt(
            paintingEntries,
            maskEntries,
            infos.authorName,
            infos.workName,
            infos.workAge,
            infos.resWidth,
            infos.resHeight,
            workImage,
            artWorkContext
        );
    }

    public (Texture2D, Texture2D) LoadNextFrame(List<ZipArchiveEntry> paintings, List<ZipArchiveEntry> masks, int index)
    {
        Texture2D paintingNext = LoadFrame(paintings[index]);
        Texture2D maskNext = LoadFrame(masks[index]);

        return (paintingNext, maskNext);
    }

    // ================= UTILS =================

    public Texture2D LoadFrame(ZipArchiveEntry entry)
    {
        string path = Path.Combine(workDir, entry.Name);
        entry.ExtractToFile(path, true);

        byte[] bytes = File.ReadAllBytes(path);
        File.Delete(path);

        Texture2D tex = new Texture2D(2, 2, format, false);
        tex.LoadImage(bytes, false);
        tex.Apply(false, false);

        return tex;
    }

    void OnDestroy()
    {
        archive?.Dispose();
    }
}
