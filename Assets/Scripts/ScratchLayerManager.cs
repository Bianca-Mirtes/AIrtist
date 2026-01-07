using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WorkData;

public class ScratchLayerManager : MonoBehaviour
{
    [Header("Render")]
    public Renderer target;

    [Header("Scratch Mask")]
    public RenderTexture activeMask;
    public VRBrushPainter brush;

    [Header("Final")]
    public ParticleSystem confetti;
    public GameObject UI;

    MaterialPropertyBlock mpb;

    private int totalFrames = 400;
    int globalFrame;
    public bool isLocal = true;
    public float diffAmount;

    Texture2D paintingCurrent;
    Texture2D paintingNext;

    Texture2D maskCurrent;
    Texture2D maskNext;

    public int GlobalFrame => globalFrame;

    public static ScratchLayerManager _instance;

    public static ScratchLayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ScratchLayerManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject("ScratchLayerManager");
                    _instance = singleton.AddComponent<ScratchLayerManager>();
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_HasFrames", 1);
    }

    public void SetInitialFrames(Texture2D currentPainting, Texture2D nextPainting, Texture2D currentMask, Texture2D nextMask)
    {
        paintingCurrent = currentPainting;
        paintingNext = nextPainting;

        maskCurrent = currentMask;
        maskNext = nextMask;

        diffAmount = ComputeDiff();

        ApplyToMaterial();
        brush.currentDiffMask = maskCurrent;
    }

    public void SetTotalFrames(int value)
    {
        totalFrames = value;
    }

    // =================== FRAME ADVANCE ===================

    public void AdvanceFrame()
    {
        if (globalFrame == totalFrames - 1)
        {
            confetti?.Play();
            UI?.SetActive(true);
            ChooseArtController.Instance.ResetBrush();
            return;
        }

        globalFrame++;
        // swap
        paintingCurrent = paintingNext;
        maskCurrent = maskNext;

        if (!isLocal) {
            WorkAPI api = (WorkAPI)ChooseArtController.Instance.GetCurrentWork();
            (Texture2D, Texture2D) newNextTex = FindFirstObjectByType<FrameZipLoader>().LoadNextFrame(api.painting, api.masks, globalFrame);
            paintingNext = newNextTex.Item1;
            maskNext = newNextTex.Item2;
        }
        else
        {
            Work work = (Work)ChooseArtController.Instance.GetCurrentWork();
            paintingNext = work.painting[globalFrame + 1];
            maskNext = work.masks[globalFrame + 1];
        }

        diffAmount = ComputeDiff();

        ApplyToMaterial();
        brush.currentDiffMask = maskCurrent;

        ClearMask();

        Debug.Log($"🖌 Frame atual: {globalFrame}");
    }

    // =================== MATERIAL ===================

    void ApplyToMaterial()
    {
        mpb.SetTexture("_MainTex", paintingCurrent);
        mpb.SetTexture("_NextTex", paintingNext);
        mpb.SetTexture("_Mask", maskCurrent);
        mpb.SetTexture("_ScratchMask", activeMask);

        target.SetPropertyBlock(mpb);
    }

    public float ComputeDiff()
    {
        var pa = paintingCurrent.GetPixels32();
        var pb = paintingNext.GetPixels32();

        int diff = 0;

        for (int i = 0; i < pa.Length; i++)
        {
            float d =
                (Mathf.Abs(pa[i].r - pb[i].r) +
                 Mathf.Abs(pa[i].g - pb[i].g) +
                 Mathf.Abs(pa[i].b - pb[i].b)) / (3f * 255f);

            if (d > 0.05f)
                diff++;
        }

        return diff / (float)pa.Length;
    }

    void ClearMask()
    {
        if (activeMask == null) return;

        var prev = RenderTexture.active;
        RenderTexture.active = activeMask;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = prev;
    }

    void OnDisable()
    {
        mpb.Clear();
        mpb.SetFloat("_HasFrames", 0);
        ClearMask();
        target.SetPropertyBlock(mpb);
    }
}
