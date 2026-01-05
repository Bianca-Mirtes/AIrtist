using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using WorkData;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ChooseArtController : MonoBehaviour
{
    public List<Work> arts;
    public List<WorkAPI> artsWithApi;
    public Transform questions;
    public GameObject tutorial;
    public Material artMat;
    public GameObject brush;

    public GameObject buttonPrefab;
    public Transform content;
    private object currentWork;

    private bool clicked = false;
    public static ChooseArtController _instance;

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

            FindFirstObjectByType<ScratchLayerManager>().SetInitialFrames(work.painting[0], work.painting[1], work.masks[0], work.masks[1]);

            FindFirstObjectByType<QuestionsController>().SetQuestions(work.author, work.awnsers);
            FindFirstObjectByType<ExplanationController>().SetExplanationStage(work.awnsers, work.image);

            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(true);

            clicked = true;
            Invoke("ResetClick", 2f);
        }
    }

    public object GetCurrentWork()
    {
        return currentWork;
    }

    public void Choose(WorkAPI work)
    {
        if (!clicked)
        {
            currentWork = work;
            brush.SetActive(true);
            ScratchLayerManager.Instance.isLocal = false;
            ScratchLayerManager.Instance.SetTotalFrames(work.painting.Count);

            Texture2D firstTex = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.painting[0]);
            Texture2D secondTex = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.painting[1]);

            Texture2D firstMask = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.masks[0]);
            Texture2D secondMask = FindFirstObjectByType<FrameZipLoader>().LoadFrame(work.masks[1]);

            FindFirstObjectByType<ScratchLayerManager>().SetInitialFrames(firstTex, secondTex, firstMask, secondMask);

            transform.GetChild(1).gameObject.SetActive(false);
            StartPractise();

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

    public void CreateNewArt(List<ZipArchiveEntry> arrays, List<ZipArchiveEntry> masks, string authorName, string workName, string workAge, Sprite workImage)
    {
        GameObject btn = Instantiate(buttonPrefab, content);
        WorkAPI work = new WorkAPI();
        work.painting = arrays;
        work.masks = masks;

        work.author = authorName;
        work.workName = workName;
        work.age = workAge;

        work.image = workImage;
        artsWithApi.Add(work);
        
        string description = work.workName + "\n" + work.author + ", " + work.age;
        btn.GetComponent<Image>().sprite = work.image;
        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = description;
        btn.GetComponent<Button>().onClick.AddListener(() => Choose(work));
    }

    public void StartPractise()
    {
        questions.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(3).gameObject.SetActive(false);
        brush.SetActive(true);
        tutorial.SetActive(true);
    }
}
