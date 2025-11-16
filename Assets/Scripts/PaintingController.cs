using UnityEngine;

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
}


