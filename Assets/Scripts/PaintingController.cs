using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static BrushGuide;

public class PaintingController : MonoBehaviour
{
    [Header("Painting Material")]
    public Material paintingMaterial;

    [Header("Blend Settings")]
    public float blendSpeed = 0.75f;

    [Header("Frame Control")]
    public int currentFrame { get; set; } = 0;
    public int totalFrames = 400;

    // Blend atual
    private float blend = 0f;
    public GameObject arrowPrefab;
    GameObject currentArrow;

    public bool isPainting { get; set; } = false;
    public static PaintingController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    /*void Update()
    {
        if (!isPainting)
            return;

        // aumenta o blend progressivamente
        blend += Time.deltaTime * blendSpeed;
        paintingMaterial.SetFloat("_Blend", blend);

        // Se completou uma transição, vai para o próximo frame
        if (blend >= 1f)
        {
            AdvanceFrame();
        }
    }*/

    // ======================================================
    // Passar para o próximo frame
    // ======================================================
    public void AdvanceFrame()
    {
        if (currentFrame >= totalFrames - 1)
        {
            return;
        }
        currentFrame++;
        paintingMaterial.SetInt("_GlobalFrame", currentFrame);
        BrushGuide.Instance.ShowGuideArrow(currentFrame, currentFrame+1);
    }
    private void OnDestroy()
    {
        paintingMaterial.SetFloat("_Blend", 0f);
        paintingMaterial.SetInt("_GlobalFrame", 0);
    }
}
