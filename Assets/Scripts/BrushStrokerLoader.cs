using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BrushStrokeLoader : MonoBehaviour
{
    [Header("Arquivo TXT / CSV")]
    public TextAsset coordinatesFile;

    [Header("Resolução da Imagem (px)")]
    public int imageWidth = 1024;
    public int imageHeight = 1024;

    [Header("Strokes carregados")]
    public List<BrushStroke> strokes = new List<BrushStroke>();


    [ContextMenu("Carregar Brush Strokes")]
    public List<BrushStroke> LoadStrokes()
    {
        strokes.Clear();

        if (coordinatesFile == null)
        {
            Debug.LogError("Nenhum arquivo TXT/CSV foi atribuído!");
            return null;
        }

        using (StringReader reader = new StringReader(coordinatesFile.text))
        {
            string line;
            bool isFirstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Ignorar a linha de cabeçalho
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                string[] c = line.Split(',');

                if (c.Length < 15)
                {
                    Debug.LogWarning("Linha inválida: " + line);
                    continue;
                }

                BrushStroke stroke = new BrushStroke();

                // Dados crus
                stroke.pairIndex = int.Parse(c[0]);
                stroke.imgFrom = c[1].Trim();
                stroke.imgTo = c[2].Trim();

                float startX = float.Parse(c[4]);
                float startY = float.Parse(c[5]);
                float endX = float.Parse(c[6]);
                float endY = float.Parse(c[7]);

                float centroidX = float.Parse(c[8]);
                float centroidY = float.Parse(c[9]);

                float dirX = float.Parse(c[10]);
                float dirY = float.Parse(c[11]);

                float lengthPx = float.Parse(c[12]);
                float angleRad = float.Parse(c[13]);
                float sizePx = float.Parse(c[14]);

                // Conversão para UV
                stroke.uvStart = new Vector2(startX / imageWidth, startY / imageHeight);
                stroke.uvEnd = new Vector2(endX / imageWidth, endY / imageHeight);
                stroke.centroidUV = new Vector2(centroidX / imageWidth, centroidY / imageHeight);

                stroke.directionUV = new Vector2(dirX, dirY).normalized;
                stroke.lengthUV = lengthPx / Mathf.Max(imageWidth, imageHeight);

                stroke.angleRad = angleRad;
                stroke.sizePixels = sizePx;

                // Frames → extraídos de generatedXYZ.png
                stroke.frameFrom = ExtractFrameNumber(stroke.imgFrom);
                stroke.frameTo = ExtractFrameNumber(stroke.imgTo);

                strokes.Add(stroke);
            }
        }

        Debug.Log($"CARREGADO: {strokes.Count} strokes.");
        return strokes;
    }


    /// <summary>
    /// Converte "generated000.png" -> 0
    /// </summary>
    private int ExtractFrameNumber(string fileName)
    {
        string noExt = Path.GetFileNameWithoutExtension(fileName);

        // Pega todos os dígitos do nome
        string digits = "";
        foreach (char c in noExt)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        if (digits.Length == 0)
            return 0;

        return int.Parse(digits);
    }
}