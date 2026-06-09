using UnityEngine;
using UnityEngine.EventSystems;

public class HoldRecorder : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private AskController askController;

    private bool recording;

    public bool askBtnNeedToBeDefined = false;

    public GameObject parent;

    private void Update()
    {
        if (askBtnNeedToBeDefined)
        {
            if (ScratchLayerManager.Instance.isLocal)
            {
                parent.gameObject.SetActive(false);
            }
            else
            {
                parent.gameObject.SetActive(true);
            }
            askBtnNeedToBeDefined = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (recording) return;

        recording = true;
        askController.StartRecording();

        Debug.Log("START");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!recording) return;

        recording = false;
        askController.Stop();

        Debug.Log("STOP");
    }
}