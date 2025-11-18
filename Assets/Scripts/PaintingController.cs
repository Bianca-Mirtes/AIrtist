/*using UnityEngine;

public class PaintingController : MonoBehaviour
{
    public RenderTexture mask;
    public Material paintMaterial;
    public Material paintingMaterial;

    private RenderTexture tempRT;

    void Start()
    {
        tempRT = new RenderTexture(mask.width, mask.height, 0, RenderTextureFormat.R8);
        tempRT.Create();

        paintingMaterial.SetTexture("_PaintMask", mask);
    }

    public void PaintAtUV(Vector2 uv, float radius)
    {
        paintMaterial.SetVector("_PaintUV", new Vector4(uv.x, uv.y, radius, 0));

        Graphics.Blit(mask, tempRT);
        paintMaterial.SetTexture("_PrevMask", tempRT);

        Graphics.Blit(tempRT, mask, paintMaterial);
    }
}*/

using UnityEngine;

public class PaintingController : MonoBehaviour
{
    public RenderTexture paintRT;
    public Material paintMaterial;
    Texture2D currentMask;


    [Header("Mask Settings")]
    public Texture2D[] maskFrames;     // array de todas as máscaras
    public Color targetMaskColor = new Color(166 / 255f, 68 / 255f, 73 / 255f);
    public float colorTolerance = 0.3f;

    public void LoadCurrentMask(int currentIndex)
    {
        if (maskFrames.Length == 0) return;
        if (currentIndex >= maskFrames.Length) return;

        currentMask = maskFrames[currentIndex];
    }


    public void PaintAtUV(Vector2 uv)
    {
        int px = Mathf.FloorToInt(uv.x * currentMask.width);
        int py = Mathf.FloorToInt(uv.y * currentMask.height);

        Color c = currentMask.GetPixel(px, py);

        if (IsRedArea(c))
        {
            Debug.Log("Pintou a area correta!");
            FindFirstObjectByType<FrameBlender>().isPainting = true;
        }
        else
        {
            Debug.Log("Pintou a area errada!");
        }
    }

    bool IsRedArea(Color c)
    {
        return c.r > 0.4f && (c.r - c.g) > 0.15f && (c.r - c.b) > 0.15f;
    }
}



