using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

public class BrushGuide : MonoBehaviour
{
    public static BrushGuide Instance;

    [System.Serializable]
    public class StrokeGuideData
    {
        public int pairIndex;
        public int imgFrom;
        public int imgTo;
        public float startX, startY;
        public float endX, endY;
        public float cx, cy;           // centroid_x / y
        public float dirX, dirY;       // unit_dir_x / y
        public float length;
        public float angle;            // angle_rad
        public int sizePixels;
    }

    [Header("Painting Quad")]
    public Renderer paintingRenderer; // mesh renderer do QUAD 3D

    [Header("Resolution Quad")]
    private int imgWidth;
    private int imgHeight;

    [Header("Arrow Prefab")]
    public GameObject arrowPrefab;
    public float arrowDistanceOffset = 0.01f; // 1cm na frente da tela

    [Header("Guide Data")]
    private Dictionary<(int, int), StrokeGuideData> guideDict;

    private CultureInfo ci;

    GameObject currentArrow;

    void Awake()
    {
        Instance = this;
        ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
    }

    public void SetResolution(int w, int h)
    {
        imgWidth = w;
        imgHeight = h;
    }


    public void LoadGuideData(TextAsset guideFile)
    {
        guideDict = new Dictionary<(int, int), StrokeGuideData>();
        string[] lines = guideFile.text.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("#")) continue;

            string[] parts = line.Split(',');

            StrokeGuideData data = new StrokeGuideData();
            data.pairIndex = int.Parse(parts[0]);
            data.imgFrom = int.Parse(parts[1].Replace("generated", "").Replace(".png", ""));
            data.imgTo = int.Parse(parts[2].Replace("generated", "").Replace(".png", ""));

            data.startX = float.Parse(parts[4], NumberStyles.Float, ci);
            data.startY = float.Parse(parts[5], NumberStyles.Float, ci);
            data.endX = float.Parse(parts[6], NumberStyles.Float, ci);
            data.endY = float.Parse(parts[7], NumberStyles.Float, ci);

            data.cx = float.Parse(parts[8], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);
            data.cy = float.Parse(parts[9], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);

            data.dirX = float.Parse(parts[10], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);
            data.dirY = float.Parse(parts[11], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);

            data.length = float.Parse(parts[12], NumberStyles.Float, ci);
            data.angle = float.Parse(parts[13], NumberStyles.Float | NumberStyles.AllowLeadingSign, ci);

            data.sizePixels = int.Parse(parts[14]);

            guideDict[(data.imgFrom, data.imgTo)] = data;
        }

        Debug.Log($"Loaded {guideDict.Count} stroke entries.");
    }

    // -------------------------------------------------------------
    // Este é o método chamado pelo AdvanceFrame()
    // -------------------------------------------------------------
    public void ShowGuideArrow(int from, int to)
    {
        if (!guideDict.TryGetValue((from, to), out StrokeGuideData data))
        {
            Debug.LogWarning($"No guide data for {from} -> {to}");
            return;
        }

        SpawnArrow(data);
    }

    void SpawnArrow(StrokeGuideData data)
    {
        if (currentArrow) Destroy(currentArrow);

        // resolução da sua pintura
        float texW = imgWidth;
        float texH = imgHeight;

        // pixel → UV
        float u = data.cx / texW;
        float v = 1f - (data.cy / texH); // flip vertical

        // UV → local do quad
        Vector3 local = new Vector3(u - 0.5f, v - 0.5f, 0f);

        Transform quad = paintingRenderer.transform;

        // local → world
        Vector3 world = quad.TransformPoint(local);

        // empurrar para frente da tela
        Vector3 normal = quad.TransformDirection(Vector3.forward);
        world += normal * arrowDistanceOffset;

        // direção da seta (em espaço local da imagem)
        Vector3 localDir = new Vector3(data.dirX, -data.dirY, 0f);
        // OBS: inverti Y para alinhar com o flip vertical de antes

        // localDir → worldDir
        Vector3 worldDir = quad.TransformDirection(localDir).normalized;

        // criar rotação
        Quaternion rot = Quaternion.LookRotation(worldDir, normal);

        // instanciar seta
        currentArrow = Instantiate(arrowPrefab, world, rot);
    }
}

