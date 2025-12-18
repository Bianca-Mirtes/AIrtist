using UnityEngine;
using System.IO;
using System.IO.Compression;

public class SimpleZipExtractor : MonoBehaviour
{
    public string zipFileName = "CapturesGames.zip";

    // DEFINA AQUI O CAMINHO QUE VOCÊ QUER!
    public string extractToFolder = "C:/MeusProjetos/ExtractedUnity";

    void Start()
    {
        ExtractToCustomLocation();
    }

    void ExtractToCustomLocation()
    {
        // Caminho do ZIP (deve estar em Assets/StreamingAssets/)
        string zipPath = Path.Combine(Application.streamingAssetsPath, zipFileName);

        // Caminho de destino (VISÍVEL)
        string targetPath = Path.Combine(extractToFolder, Path.GetFileNameWithoutExtension(zipFileName));

        Debug.Log($"Extraindo de: {zipPath}");
        Debug.Log($"Para: {targetPath}");

        // Verifica se o ZIP existe
        if (!File.Exists(zipPath))
        {
            Debug.LogError($"ZIP não encontrado! Coloque em: {Application.streamingAssetsPath}");
            return;
        }

        // Cria pasta se não existir
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        // Extrai
        ZipFile.ExtractToDirectory(zipPath, targetPath, true);

        Debug.Log($"<color=green>Extraído com sucesso para: {targetPath}</color>");

        // Abre a pasta no explorador
        OpenFolder(targetPath);
    }

    void OpenFolder(string path)
    {
        path = path.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", path);
    }

    // Botão no Inspector
    [ContextMenu("Extrair Agora")]
    void ExtractNow()
    {
        ExtractToCustomLocation();
    }
}