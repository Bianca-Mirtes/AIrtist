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

    public int totalFrames;
    public int globalFrame = 0;
    public bool isLocal = true;
    //public float diffAmount;

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

    private void Start()
    {
        UI?.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Button>().onClick.AddListener(ResetPaint);
    }

    private void ResetPaint()
    {
        UI.SetActive(false);
        ChooseArtController.Instance.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        ClearMask();
        mpb.Clear();
    }

    public void SetInitialFrames(Texture2D currentPainting, Texture2D nextPainting, Texture2D currentMask, Texture2D nextMask, int total)
    {
        totalFrames = total;
        paintingCurrent = currentPainting;
        paintingNext = nextPainting;

        maskCurrent = currentMask;
        maskNext = nextMask;

        //diffAmount = ComputeDiff();

        //activeMask.width = maskCurrent.width;
        //activeMask.height = maskCurrent.height;

        brush.canPaint = true;

        ApplyToMaterial();
        brush.analyzer.ConvertToR8(maskCurrent);
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
            mpb.SetFloat("_IsTheLastFrame", 1);
            paintingCurrent = paintingNext;
            maskCurrent = maskNext;

            ApplyToMaterial();

            confetti?.Play();
            UI?.SetActive(true);
            brush.canPaint = false;
            ChooseArtController.Instance.isRunningWork = false;
            ChooseArtController.Instance.ResetBrush();
            return;
        }

        globalFrame++;

        Destroy(paintingCurrent);
        Destroy(maskCurrent);

        // swap
        paintingCurrent = paintingNext;
        maskCurrent = maskNext;

        if (!isLocal) {
            WorkAPI api = ChooseArtController.Instance.GetCurrentWorkAPI();
            (Texture2D, Texture2D) newNextTex = FindFirstObjectByType<FrameZipLoader>().LoadNextFrame(api.painting, api.masks, globalFrame);
            paintingNext = newNextTex.Item1;
            maskNext = newNextTex.Item2;
        }
        else
        {
            Work work = ChooseArtController.Instance.GetCurrentWork();
            paintingNext = work.painting[globalFrame + 1];
            maskNext = work.masks[globalFrame + 1];
        }

        //diffAmount = ComputeDiff();

        ApplyToMaterial();
        brush.analyzer.ConvertToR8(maskCurrent);

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
        brush.analyzer.Dispose();
        mpb.SetFloat("_HasFrames", 0);
        ClearMask();
        target.SetPropertyBlock(mpb);
    }
}
