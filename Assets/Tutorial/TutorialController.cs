using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public Sprite[] frames;
    public Image image;

    public void SetFrame(int value)
    {
        image.sprite = frames[value];
    }
}
