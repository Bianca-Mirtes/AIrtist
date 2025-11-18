using UnityEngine;

public class FrameBlender : MonoBehaviour
{
    public Material paintingMaterial;

    public float blendSpeed = 0.75f;

    private float currentBlend = 0f;
    public int currentFrame = 0;  // 0–399

    private const int batchSize = 100;
    private const int totalFrames = 400;

    public bool isPainting { get; set; } = false;

    private void Start()
    {
        //FindFirstObjectByType<PaintingController>().LoadCurrentMask(currentFrame);
    }

    public void UpdateFrame()
    {
        // aumenta o blend progressivamente
        currentBlend += Time.deltaTime * 1;
        paintingMaterial.SetFloat("_Blend", currentBlend);

        // terminou a transição?
        if (currentBlend >= 1f)
        {
            currentBlend = 0f;

            // avança para o próximo frame global
            currentFrame++;
            //FindFirstObjectByType<PaintingController>().LoadCurrentMask(currentFrame);

            if (currentFrame >= totalFrames - 1)
                currentFrame = totalFrames - 2; // trava antes do último para não estourar
        }

        // enviar para o shader
        paintingMaterial.SetInt("_GlobalFrame", currentFrame);
    }
    private void OnDestroy()
    {
        currentBlend = 0f;
        currentFrame = 0;
        paintingMaterial.SetFloat("_Blend", currentBlend);
        paintingMaterial.SetInt("_GlobalFrame", currentFrame);
    }

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
            currentFrame++;
            //FindFirstObjectByType<PaintingController>().LoadCurrentMask(currentFrame);

            if (currentFrame >= totalFrames - 1)
                currentFrame = totalFrames - 2; // trava antes do último para não estourar
        }

        // enviar para o shader
        paintingMaterial.SetInt("_GlobalFrame", currentFrame);
    }
}

