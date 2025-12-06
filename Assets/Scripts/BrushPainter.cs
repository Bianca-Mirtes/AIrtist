using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static BrushGuide;

public class BrushPainter : MonoBehaviour
{
    public LayerMask paintSurfaceLayer;
    public Renderer screenRenderer;
    public Transform tip;
    private Vector3 lastPos;

    private string folderName = "";
    private HashSet<Vector2Int> pixelHash = new HashSet<Vector2Int>();

    public float angleToleranceDeg = 25f;
    public Vector3 currentBrushDirection;

    void OnTriggerEnter(Collider other)
    {
        ArrowTarget arrow = other.GetComponent<ArrowTarget>();
        // enconstou na seta
        if (arrow != null)
        {
            if (DirectionMatches(arrow.requiredAngle))
            {
                Debug.Log("✔ Pintou na direção certa — mudar frame!");
                PaintingController.Instance.AdvanceFrame();
            }
            else
            {
                Debug.Log("❌ Direção errada — não avança");
            }
        }
    }

    bool DirectionMatches(float targetAngle)
    {
        float brushAngle = Mathf.Atan2(currentBrushDirection.y, currentBrushDirection.x);

        float delta = Mathf.Abs(Mathf.DeltaAngle(
            brushAngle * Mathf.Rad2Deg,
            targetAngle * Mathf.Rad2Deg
        ));

        return delta <= angleToleranceDeg;
    }

    void Update()
    {
        Vector3 currentPos = tip.position;
        currentBrushDirection = (currentPos - lastPos).normalized;
        lastPos = currentPos;
    }


    /*void Update()
    {
        if (Physics.Raycast(tip.position, tip.forward, out RaycastHit hit, 0.05f, paintSurfaceLayer))
        {
            Vector2 uv = hit.textureCoord;
            Vector3 worldPos = hit.point;

            OnBrushTouchScreen(uv, worldPos);
        }
    }

    void OnBrushTouchScreen(Vector2 uv, Vector3 worldPos)
    {
        int px = Mathf.FloorToInt(uv.x * imgWidth);
        int py = Mathf.FloorToInt(uv.y * imgHeight);

        if (pixelHash.Contains(new Vector2Int(px, py)))
        {
            //Instantiate(markPrefab, worldPos, Quaternion.Euler(0, 90f, 0f));
            PaintingController.Instance.isPainting = true;
            Debug.Log($"✔ Acertou o ponto correto: {px},{py}");
            //ProgressInFrame();
            return;
        }else
            PaintingController.Instance.isPainting = false;
    }


    public void LoadNextFramePixels()
    {
        pixelHash.Clear();

        string filename = $"generated{PaintingController.Instance.currentFrame:000}_to_generated{PaintingController.Instance.currentFrame + 1:000}_oval_pixels.txt";
        folderName = folderName.Replace(" ", "_");

        string fullPath = Path.Combine(Application.streamingAssetsPath, folderName, filename);

        Debug.Log("Carregando arquivo: " + fullPath);

        string[] lines = File.ReadAllLines(fullPath);

        foreach (string line in lines)
        {
            if (line.StartsWith("#")) continue;
            if (!line.Contains(",")) continue;

            string[] split = line.Split(',');
            int x = int.Parse(split[0]);
            int y = int.Parse(split[1]);

            pixelHash.Add(new Vector2Int(x, y));
        }

        Debug.Log("Pixels carregados: " + pixelHash.Count);
    }*/
}

