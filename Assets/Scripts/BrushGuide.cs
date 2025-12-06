using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class BrushGuide : MonoBehaviour
{
    private Dictionary<(int from, int to), StrokeGuideData> guideMap;
    public static BrushGuide Instance { get; private set; }

    [System.Serializable]
    public struct StrokeGuideData
    {
        public float cx, cy;
        public float dirX, dirY;
        public float angle;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        guideMap = new Dictionary<(int, int), StrokeGuideData>();
    }

    public void LoadStrokeData(TextAsset file)
    {
        guideMap.Clear();
        var lines = file.text.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("#")) continue;

            var parts = line.Split(',');
            // trim whitespaces
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();

            string imgFrom = parts[1]; // generated000.png
            string imgTo = parts[2]; // generated001.png

            int f = ExtractIndex(imgFrom);
            int t = ExtractIndex(imgTo);

            var ci = CultureInfo.InvariantCulture;
            StrokeGuideData data = new StrokeGuideData();
            data.cx = float.Parse(parts[8], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);
            data.cy = float.Parse(parts[9], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);
            data.dirX = float.Parse(parts[10], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);
            data.dirY = float.Parse(parts[11], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);
            data.angle = float.Parse(parts[13], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);
            Debug.Log(data.cx);
            Debug.Log(data.cy);
            Debug.Log(data.dirX);
            Debug.Log(data.dirY);
            Debug.Log(data.angle);

            guideMap[(f, t)] = data;
        }
    }

    public void ShowGuideArrow(int currentFrame, int nextFrame)
    {
        if (!guideMap.TryGetValue((currentFrame, nextFrame), out StrokeGuideData data))
        {
            Debug.LogWarning("⚠ Não há vetor guia para esta transição.");
            return;
        }

        SpawnArrow(data, currentFrame, nextFrame);
    }

    public GameObject arrowPrefab;
    private GameObject currentArrow;
    public Transform screenRenderer;
    public int imgWidth;
    public int imgHeight;

    public void SetResolution(int width, int height)
    {
        imgWidth = width;
        imgHeight = height;
    }

    private void SpawnArrow(StrokeGuideData d, int currentFrame, int nextFrame)
    {
        // apagar seta anterior
        if (currentArrow != null)
            Destroy(currentArrow);

        Vector2 uv = new Vector2(
            d.cx / imgWidth,
            1f - (d.cy / imgHeight)
        );

        Debug.Log("Centroid x: "+ d.cx);
        Debug.Log("Centroid y: " + d.cy);

        Debug.Log("uv x: " + uv.x);
        Debug.Log("uv y: " + uv.y);

        Vector3 worldPos = PixelToWorld(uv.x, uv.y, screenRenderer);

        float angleDeg = d.angle * Mathf.Rad2Deg;

        Quaternion rot = Quaternion.Euler(
            -angleDeg,
            0f,
            0f
        );

        GameObject arrow = Instantiate(arrowPrefab, worldPos, rot);

        ArrowTarget info = arrow.GetComponent<ArrowTarget>();
        info.fromFrame = currentFrame;
        info.toFrame = nextFrame;
        info.requiredAngle = d.angle;

        currentArrow = arrow; // store for later cleanup
    }
    public Vector3 PixelToWorld(float pixelX, float pixelY, Transform quad)
    {
        // Pixel → UV 0..1
        float u = pixelX / imgWidth;
        float v = pixelY / imgHeight;

        // Inverter Y porque Unity usa bottom→top
        v = 1f - v;

        // UV → offset [-0.5..+0.5]
        float offX = (u - 0.1f) * quad.localScale.x;
        float offY = (v - 0.5f) * quad.localScale.y;

        // Constrói posição no mundo usando os vetores do quad
        Vector3 world =
            quad.position + new Vector3(0.09f, 0f, 0f) +
            quad.right * offX +
            quad.up * offY;
        Debug.Log("offX = " + offX);
        Debug.Log("offY = " + offY);
        Debug.Log("quadR = " + quad.right);
        Debug.Log("quadU = " + quad.up);

        return world;
    }

    int ExtractIndex(string img)
    {
        // generated000.png → 0
        string num = img.Replace("generated", "").Replace(".png", "");
        return int.Parse(num);
    }
}

