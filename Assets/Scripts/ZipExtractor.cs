using System.IO;
using System.IO.Compression;
using UnityEngine;

public static class ZipExtractor
{
    public static void ExtractIfNeeded(string zipPath, string extractDir)
    {
        try
        {
            if (!Directory.Exists(extractDir))
            {
                Debug.Log($"Criando diretório: {extractDir}");
                Directory.CreateDirectory(extractDir);
            }

            // Verifica se já foi extraído (verifica se há arquivos no diretório)
            string[] files = Directory.GetFiles(extractDir);
            string[] directories = Directory.GetDirectories(extractDir);

            if (files.Length == 0 && directories.Length == 0)
            {
                Debug.Log($"Extraindo {zipPath} para {extractDir}");

                // Usar ZipArchive para maior controle
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        try
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(extractDir, entry.FullName));

                            // Garante que o diretório de destino exista
                            string directory = Path.GetDirectoryName(destinationPath);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            // Se não for diretório, extrai o arquivo
                            if (!string.IsNullOrEmpty(entry.Name))
                            {
                                entry.ExtractToFile(destinationPath, true);
                                Debug.Log($"Extraído: {entry.FullName}");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"Erro ao extrair {entry.FullName}: {ex.Message}");
                        }
                    }
                }
                Debug.Log("Extração completa!");
            }
            else
            {
                Debug.Log("Arquivos já extraídos anteriormente.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erro na extração: {ex.Message}\n{ex.StackTrace}");
        }
    }
}