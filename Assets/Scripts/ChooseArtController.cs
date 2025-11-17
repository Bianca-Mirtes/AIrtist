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
        artMat.SetTexture("_Frames", work.texture);
    }

    public void StartPractise()
    {
        questions.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(false);
        tutorial.SetActive(true);
    }

    private void OnDestroy()
    {
        artMat.SetTexture("_Frames", null);
    }
}
