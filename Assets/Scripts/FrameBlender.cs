using UnityEngine;

public class FrameBlender : MonoBehaviour
{
    public Material paintingMaterial;

    public float blendSpeed = 0.75f;

    private float currentBlend = 0f;
    private int globalFrame = 0;  // 0–399

    private const int batchSize = 100;
    private const int totalFrames = 400;

    public bool isPainting { get; set; } = false;

    void Update()
    {
        if (!isPainting)
            return;

        // aumenta o blend progressivamente
        currentBlend += Time.deltaTime * blendSpeed;
        paintingMaterial.SetFloat("_Blend", currentBlend);

        // terminou a transição?
        if (currentBlend >= 1f)
        {
            currentBlend = 0f;

            // avança para o próximo frame global
            globalFrame++;
            FindFirstObjectByType<PaintingController>().NextFrame();

            if (globalFrame >= totalFrames - 1)
                globalFrame = totalFrames - 2; // trava antes do último para não estourar
        }

        // enviar para o shader
        paintingMaterial.SetInt("_GlobalFrame", globalFrame);
    }
}

