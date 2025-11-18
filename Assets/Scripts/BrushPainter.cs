using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BrushPainter : MonoBehaviour
{
    public XRGrabInteractable grab;
    public Transform tip;
    public float brushRadius = 0.01f;
    public LayerMask paintableLayer;

    private PaintingController target;

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // nada a fazer
    }

    void OnRelease(SelectExitEventArgs args)
    {
        target = null;
    }

    void Update()
    {
        if (!grab.isSelected)
            return;

        // SphereCast aumenta MUITO a detecção no Quest
        if (Physics.SphereCast(tip.position, brushRadius * 0.5f, -tip.up, out RaycastHit hit, 0.02f, paintableLayer))
        {
            var receiver = hit.collider.GetComponent<PaintingController>();

            if (receiver != null)
            {
                target = receiver;

                Vector2 uv = hit.textureCoord;
                //target.PaintAtUV(uv);
                //FindFirstObjectByType<FrameBlender>().UpdateFrame();
                FindFirstObjectByType<FrameBlender>().isPainting = true;
            }
            else
            {
                FindFirstObjectByType<FrameBlender>().isPainting = false;
            }
        }
    }
}
