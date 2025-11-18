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

using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class PaintingController : MonoBehaviour
{
    [Header("Pintura do usuário")]
    public RenderTexture mask;                // Onde o usuário pinta
    public Material paintMaterial;            // Material usado p/ pintar no mask
    public float radius = 0.01f;

    [Header("Máscaras de Validação (Frames)")]
    public Texture2D[] validationMasks;       // Suas 400 imagens com vermelho
    public int currentFrame = 0;

    public List<BrushStroke> strokes;

    private RenderTexture tempRT;

    // Cor alvo do vermelho da máscara
    private Color targetRed = new Color(152 / 255f, 29 / 255f, 33 / 255f);

    // Tolerância porque compressão de textura pode alterar alguns decimais
    private const float colorTolerance = 0.1f;

    void Start()
    {
        tempRT = new RenderTexture(mask.width, mask.height, 0, RenderTextureFormat.R8);
        tempRT.Create();

        strokes = FindFirstObjectByType<BrushStrokeLoader>().LoadStrokes();
    }

    /// <summary>
    /// Pinta na máscara e valida
    /// </summary>
    public void PaintAtUV(Vector2 uv)
    {
        // ----- PINTAR NO MASK -----
        paintMaterial.SetTexture("_MaskTex", mask);
        paintMaterial.SetVector("_PaintUV", new Vector4(uv.x, uv.y, radius, 0));

        Graphics.Blit(mask, tempRT, paintMaterial);
        Graphics.Blit(tempRT, mask);

        // ----- VALIDAR PINTURA -----
        Debug.Log("Validando pintura");
        ValidatePainting(uv);
    }

    public static bool IsPointNearStroke(Vector2 point, Vector2 a, Vector2 b, float maxDist)
    {
        Vector2 ap = point - a;
        Vector2 ab = b - a;

        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);

        Vector2 closest = a + t * ab;
        float dist = Vector2.Distance(point, closest);

        return dist <= maxDist;
    }


    /// <summary>
    /// Valida se o usuário pintou na área correta da máscara do frame atual
    /// </summary>
    private void ValidatePainting(Vector2 uv)
    {
        var stroke = strokes[currentFrame];

        bool ok = IsPointNearStroke(
            uv,
            stroke.uvStart,
            stroke.uvEnd,
            0.03f
        );

        if (ok)
        {
            Debug.Log("Stroke OK!");

            FindFirstObjectByType<FrameBlender>().isPainting = true;
        }
        else
        {
            Debug.Log("Não é a area!");
            // opcional: feedback de erro
            FindFirstObjectByType<FrameBlender>().isPainting = false;
        }

        /*Texture2D validationTex = validationMasks[currentFrame];

        if (validationTex == null)
        {
            Debug.LogWarning("Mask de validação não encontrada para o frame: " + currentFrame);
            return;
        }

        // Converter UV -> pixel
        int x = (int)(uv.x * validationTex.width);
        int y = (int)(uv.y * validationTex.height);

        // Segurança
        x = Mathf.Clamp(x, 0, validationTex.width - 1);
        y = Mathf.Clamp(y, 0, validationTex.height - 1);

        Color c = validationTex.GetPixel(x, y);

        if (IsRedTarget(c))
        {
            Debug.Log("🎯 Acertou área correta!");
            FindFirstObjectByType<FrameBlender>().isPainting = true;
            // Aqui você pode somar pontos, atualizar progresso, etc.
        }
        else
        {
            Debug.Log("❌ Pintou fora da área correta");
            FindFirstObjectByType<FrameBlender>().isPainting = false;
        }*/
    }

    /// <summary>
    /// Checa se um pixel é próximo do vermelho alvo
    /// </summary>
    private bool IsRedTarget(Color c)
    {
        return
            Mathf.Abs(c.r - targetRed.r) < colorTolerance &&
            Mathf.Abs(c.g - targetRed.g) < colorTolerance &&
            Mathf.Abs(c.b - targetRed.b) < colorTolerance;
    }

    /// <summary>
    /// Quando a região for completada, chame isto.
    /// </summary>
    public void NextFrame()
    {
        currentFrame++;
        if (currentFrame >= validationMasks.Length)
        {
            currentFrame = validationMasks.Length - 1;
            Debug.Log("🎉 Todos os frames completos!");
        }
        else
        {
            Debug.Log("➡ Avançou para frame " + currentFrame);
        }
    }
}


