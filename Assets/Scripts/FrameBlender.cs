using UnityEngine;

public class FrameBlender : MonoBehaviour
{
    public Material paintingMaterial;  // Material que usa PaintingRevealArray_Builtin.shader

    public float blendSpeed = 0.75f;

    private float currentBlend = 0f;
    private int frameIndex = 0;

    private const int maxFrames = 500;
    public bool isPainting { set; get; } = false;

    void Update()
    {
        if (isPainting)
        {
            // aumenta o blend progressivamente
            currentBlend += Time.deltaTime * blendSpeed;
            paintingMaterial.SetFloat("_Blend", currentBlend);

            // terminou a transição?
            if (currentBlend >= 1f)
            {
                currentBlend = 0f;

                // avança para o próximo frame
                frameIndex++;

                if (frameIndex >= maxFrames - 1)
                    frameIndex = maxFrames - 2;  // evita ultrapassar o limite

                paintingMaterial.SetFloat("_FrameIndex", frameIndex);
            }
        }
    }
}

