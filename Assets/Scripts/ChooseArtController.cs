using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using WorkData;

public class ChooseArtController : MonoBehaviour
{
    public List<Work> arts;
    public List<WorkAPI> artsWithApi;
    public Transform questions;
    public GameObject tutorial;
    public Material artMat;
    public GameObject brush;

    public HoldRecorder holdRecorder;

    public GameObject buttonPrefab;
    public Transform content;
    private Work currentWork = null;
    private WorkAPI currentWorkAPI = null;

    public static ChooseArtController _instance;

    [Header("Booleans")]
    public bool isRunningWorkWithAPI = false;
    public bool isRunningWork = false;
    private bool clicked = false;

    public static ChooseArtController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ChooseArtController>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject("ChooseArtController");
                    _instance = singleton.AddComponent<ChooseArtController>();
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    private void Start()
    {
        // pré gerados
        foreach (var work in arts)
        {
            GameObject btn = Instantiate(buttonPrefab, content);
            string description = work.workName + "\n" + work.author + ", " + work.age;
            btn.GetComponent<Image>().sprite = work.image;
            btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = description;
            btn.GetComponent<Button>().onClick.AddListener(() => Choose(work));
        }

        // brush
        brush.SetActive(false);
    }

    public void Choose(Work work)
    {
        if (!clicked)
        {
            currentWork = work;
            
            ScratchLayerManager.Instance.isLocal = true;
            ScratchLayerManager.Instance.SetTotalFrames(work.painting.Count);

            artMat.SetTexture("_MainTex", work.painting[0]);
            artMat.SetTexture("_NextTex", work.painting[1]);
            artMat.SetTexture("_Mask", work.masks[0]);

            FindFirstObjectByType<QuestionsController>().SetQuestions(work.author, work.awnsers);
            transform.GetChild(3).gameObject.SetActive(true);
            FindFirstObjectByType<ExplanationController>().SetExplanationStage(work.awnsers, work.image);

            transform.GetChild(1).gameObject.SetActive(false);
            //StartPractise();

            RecordingController.Instance.canRecording = false;
            isRunningWork = true;

            holdRecorder.askBtnNeedToBeDefined = true;

            isRunningWorkWithAPI = false;

            clicked = true;
            Invoke("ResetClick", 2f);
        }
    }

    public Work GetCurrentWork()
    {
        return currentWork;
    }

    public WorkAPI GetCurrentWorkAPI()
    {
        return currentWorkAPI;
    }

    public void Choose(WorkAPI work)
    {
        if (!clicked)
        {
            currentWorkAPI = work;
            brush.SetActive(true);
            ScratchLayerManager.Instance.isLocal = false;
            ScratchLayerManager.Instance.SetTotalFrames(work.painting.Count);

            Texture2D firstTex = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.painting[0]);
            Texture2D secondTex = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.painting[1]);

            Texture2D firstMask = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.masks[0]);
            Texture2D secondMask = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.masks[1]);

            FindFirstObjectByType<ScratchLayerManager>().SetInitialFrames(firstTex, secondTex, firstMask, secondMask, work.painting.Count);

            transform.GetChild(1).gameObject.SetActive(false);
            StartPractise();

            holdRecorder.askBtnNeedToBeDefined = true;

            isRunningWork = true;
            isRunningWorkWithAPI = true;

            clicked = true;
            Invoke("ResetClick", 2f);
        }
    }

    private void ResetClick()
    {
        clicked = false;
    }

    public void ResetBrush()
    {
        brush.gameObject.transform.position = new Vector3(-0.479f, 0.785f, -1.011f);
        brush.gameObject.SetActive(false);
    }

    public void CreateNewArt(List<ZipArchiveEntry> arrays, List<ZipArchiveEntry> masks, string authorName, string workName, string workAge, int resWidth, int resHeight, Sprite workImage, ArtWorkContext artWorkContext)
    {
        GameObject btn = Instantiate(buttonPrefab, content);
        WorkAPI work = new WorkAPI();
        work.painting = arrays;
        work.masks = masks;

        work.author = authorName;
        work.workName = workName;
        work.age = workAge;
        work.resWidth = resWidth;
        work.resHeight = resHeight;

        work.image = workImage;
        work.artWorkContext = artWorkContext;
        artsWithApi.Add(work);
        
        string description = workName + "\n" + authorName + ", " + workAge;
        btn.GetComponent<Image>().sprite = work.image;
        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = description;
        btn.GetComponent<Button>().onClick.AddListener(() => Choose(work));
    }

    public void StartPractise()
    {
        transform.GetChild(3).gameObject.SetActive(false);

        if (isRunningWorkWithAPI)
        {
            questions.gameObject.SetActive(false);
            ScratchLayerManager.Instance.SetInitialFrames(FindFirstObjectByType<FrameZipLoader>().LoadFrame(currentWorkAPI.painting[0]),
                FindFirstObjectByType<FrameZipLoader>().LoadFrame(currentWorkAPI.painting[1]),
                FindFirstObjectByType<FrameZipLoader>().LoadFrame(currentWorkAPI.masks[0]),
                FindFirstObjectByType<FrameZipLoader>().LoadFrame(currentWorkAPI.masks[1]),
                currentWorkAPI.painting.Count);
        }
        else
        {
            questions.gameObject.SetActive(true);
            ScratchLayerManager.Instance.SetInitialFrames(currentWork.painting[0], currentWork.painting[1], currentWork.masks[0], currentWork.masks[1], currentWork.painting.Count);
        }

        brush.SetActive(true);
        tutorial.SetActive(true);
    }
}
