using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseArtController : MonoBehaviour
{
    public Work[] arts;
    public Transform questions;
    public GameObject tutorial;
    public Material artMat;

    public GameObject buttonPrefab;
    public Transform content;

    private bool clicked = false;

    private void Start()
    {
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
            artMat.SetTexture("_LayerA", work.texture[0]);
            artMat.SetTexture("_LayerB", work.texture[1]);

 
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
