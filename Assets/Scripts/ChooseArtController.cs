using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ChooseArtController : MonoBehaviour
{
    public List<Work> arts;
    public Transform questions;
    public GameObject tutorial;
    public Material artMat;

    public GameObject buttonPrefab;
    public Transform content;

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
                    GameObject singleton = new GameObject("DialogueManager");
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
    }

    public void Choose(Work work)
    {
        if (!clicked)
        {
            artMat.SetTexture("_LayerA", work.painting[0]);
            artMat.SetTexture("_LayerB", work.painting[1]);
            artMat.SetTexture("_LayerA_DiffMasks", work.masks[0]);
            artMat.SetTexture("_LayerB_DiffMasks", work.masks[1]);

            FindFirstObjectByType<ScratchLayerManager>().SetArrays(work.painting[0], work.painting[1], work.masks[0], work.masks[1]);

            FindFirstObjectByType<QuestionsController>().SetQuestions(work.author, work.awnsers);
            //FindFirstObjectByType<ExplanationController>().SetExplanationStage(work.awnsers, work.image);

            transform.GetChild(1).gameObject.SetActive(false);
            //transform.GetChild(3).gameObject.SetActive(true);

            clicked = true;
            Invoke("ResetClick", 2f);
        }
    }

    private void ResetClick()
    {
        clicked = false;
    }

    public void CreateNewArt(Texture2DArray arrayA, Texture2DArray arrayB, Texture2DArray maskA, Texture2DArray maskB, string authorName, string workName, string workAge, Sprite workImage)
    {
        GameObject btn = Instantiate(buttonPrefab, content);
        Work work = new Work();
        work.painting[0] = arrayA;
        work.painting[1] = arrayB;
        work.masks[0] = maskA;
        work.masks[1] = maskB;

        work.author = authorName;
        work.workName = workName;
        work.age = workAge;
        work.image = workImage;
        arts.Add(work);
        
        string description = work.workName + "\n" + work.author + ", " + work.age;
        btn.GetComponent<Image>().sprite = work.image;
        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = description;
        btn.GetComponent<Button>().onClick.AddListener(() => Choose(work));
    }

    public void StartPractise()
    {
        questions.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(3).gameObject.SetActive(false);
        tutorial.SetActive(true);
    }

    private void OnDestroy()
    {
        artMat.SetInt("_FrameIndex", 0);
        artMat.SetInt("_UseLayerB", 0);
    }
}
