using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCompressor : EditorWindow
{
    string folderPath = "Assets/Starry_Night/1"; // caminho da pasta

    [MenuItem("Tools/Compress PNG Folder")]
    static void OpenWindow()
    {
        GetWindow<TextureCompressor>("Texture Compressor");
    }

    void OnGUI()
    {
        GUILayout.Label("Compress PNG Folder", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Folder Path:", folderPath);

        if (GUILayout.Button("Compress All PNGs"))
        {
            CompressAll();
        }
    }

    void CompressAll()
    {
        string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string assetPath = file.Replace("\\", "/");

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);

            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.isReadable = false;
            importer.mipmapEnabled = false;

#if UNITY_ANDROID
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
            {
                name = "Android",
                maxTextureSize = 2048,
                format = TextureImporterFormat.ETC2_RGBA8,
                overridden = true
            });
#else
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
            {
                name = "Standalone",
                maxTextureSize = 2048,
                format = TextureImporterFormat.DXT5,
                overridden = true
            });
#endif

            importer.SaveAndReimport();
        }

        Debug.Log("✔ Todas as texturas comprimidas com sucesso!");
    }
}
