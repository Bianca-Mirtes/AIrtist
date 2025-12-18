using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ScratchLayerManager : MonoBehaviour
{
    [Header("Render")]
    public Renderer target;

    [Header("Frames")]
    public int framesPerArray = 200;

    [Header("Scratch Mask")]
    public RenderTexture activeMask;

    MaterialPropertyBlock mpb;
    int globalFrame;

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
        }
        else
        {
            localFrame = globalFrame - framesPerArray;
            useLayerB = 1;
        }

        mpb.SetInt("_FrameIndex", localFrame);
        mpb.SetInt("_UseLayerB", useLayerB);

        Debug.Log($"Frame atual: {globalFrame}");

        target.SetPropertyBlock(mpb);

        ClearMask();
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
