using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ChooseArtController : MonoBehaviour
{
    public Texture2DArray[] arts;
    public Transform questions;
    public GameObject tutorial;
    public Material artMat;

    public void Choose(int value)
    {
        artMat.SetTexture("_Frames", arts[value]);

        questions.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(false);
        tutorial.SetActive(true);
    }

    private void OnDestroy()
    {
        artMat.SetTexture("_Frames", null);
    }
}
