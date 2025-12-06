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

        string zip = Application.streamingAssetsPath + "/data.zip";
        string folder = Application.persistentDataPath + "/unzipped";

        ZipExtractor.ExtractIfNeeded(zip, folder);

        // depois
        string imgPath = folder + "/textures/image1.png";

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

