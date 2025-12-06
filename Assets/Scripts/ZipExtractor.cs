using UnityEngine;
using System.IO;
using System.IO.Compression;

public static class ZipExtractor
{
    public static void ExtractIfNeeded(string zipPath, string extractDir)
    {
        if (!Directory.Exists(extractDir))
        {
            Directory.CreateDirectory(extractDir);
            ZipFile.ExtractToDirectory(zipPath, extractDir);
        }
    }
}

