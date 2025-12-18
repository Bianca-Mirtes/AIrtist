using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button exitBtn;
    public Button chooseBtn;
    public Button askBtn;

    public GameObject chooseCanvas;
    public GameObject askCanvas;

    private void Start()
    {
        exitBtn.onClick.AddListener(ExitApp);
        chooseBtn.onClick.AddListener(ChooseArt);
        askBtn.onClick.AddListener(AskForArt);
    }

    private void ChooseArt()
    {
        chooseCanvas.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void AskForArt()
    {
        askCanvas.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void ExitApp()
    {
        Application.Quit();
    }
}

