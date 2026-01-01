using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScratchLayerManager : MonoBehaviour
{
    [Header("Render")]
    public Renderer target;

    [Header("Frames")]
    public int framesPerArray = 200;
    public int totalFrames = 400;
    public Texture2DArray layerA, layerB;

    [Header("Scratch Mask")]
    public RenderTexture activeMask;
    public Texture2DArray layerA_DiffMasks;
    public Texture2DArray layerB_DiffMasks;

    public VRBrushPainter brush;

    [Header("Final")]
    public ParticleSystem confetti;
    public GameObject UI;

    MaterialPropertyBlock mpb;
    int globalFrame=0;

    public int GlobalFrame => globalFrame;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    public void AdvanceFrame()
    {
        globalFrame++;

        int localFrame;
        int useLayerB;

        if (globalFrame < framesPerArray)
        {
            localFrame = globalFrame;
            useLayerB = 0;
            ExtractDiffMaskSlice(layerA_DiffMasks, localFrame);
        }
        else
        {
            localFrame = globalFrame - framesPerArray;
            useLayerB = 1;
            ExtractDiffMaskSlice(layerB_DiffMasks, localFrame);
        }

        mpb.SetInt("_FrameIndex", localFrame);
        mpb.SetInt("_UseLayerB", useLayerB);
        target.SetPropertyBlock(mpb);

        // 🔹 limpa máscara UMA vez apenas
        ClearMask();

        if (useLayerB == 1 && globalFrame >= 399)
        {
            confetti.Play();
            UI.SetActive(true);
        }

        Debug.Log($"Frame atual: {globalFrame}");
    }

    public void SetArrays(Texture2DArray array1, Texture2DArray array2, Texture2DArray mask1, Texture2DArray mask2)
    {
        layerA = array1;
        layerB = array2;
        layerA_DiffMasks = mask1;
        layerB_DiffMasks = mask2;

        ExtractDiffMaskSlice(layerA, globalFrame);
    }

    void ExtractDiffMaskSlice(Texture2DArray src, int slice)
    {
        Texture2D tex = new Texture2D(
            src.width,
            src.height,
            TextureFormat.R8,
            false,
            true
        );

        Graphics.CopyTexture(src, slice, 0, tex, 0, 0);
        tex.Apply(false, false);

        brush.currentDiffMask = tex;
    }


    void ClearMask()
    {
        if (activeMask == null) return;

        var prev = RenderTexture.active;
        RenderTexture.active = activeMask;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = prev;
    }
}
