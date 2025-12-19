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

    [Header("Final")]
    public ParticleSystem confetti;
    public GameObject UI;

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


    void ClearMask()
    {
        if (activeMask == null) return;

        var prev = RenderTexture.active;
        RenderTexture.active = activeMask;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = prev;
    }
}
