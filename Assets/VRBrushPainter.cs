using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRBrushPainter : MonoBehaviour
{
    [Header("Brush Setup")]
    public Transform brushTip;
    public float maxDistance = 0.1f;
    public float brushSize = 0.1f;
    public Texture2D brushTexture;
    public Color paintColor = Color.white;

    [Header("Paint Target")]
    public RenderTexture activeMask;

    [Header("Stroke Control")]
    public float strokeInterval = 0.03f;

    [Header("Scratch System")]
    public ScratchLayerManager scratchManager;

    List<InputDevice> devices = new();
    Material paintMat;
    BrushStampAnalyzer analyzer;
    float lastStrokeTime;

    void Start()
    {
        paintMat = new Material(Shader.Find("Hidden/BrushPainter"));
        analyzer = new BrushStampAnalyzer();
    }

    void Update()
    {
        InputDeviceCharacteristics leftHandCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(leftHandCharacteristics, devices);
        devices[0].TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        if (triggerValue > 0.2f)
        {
            Ray ray = new Ray(brushTip.position, brushTip.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                TryPaint(hit.textureCoord);
            }
        }
    }
    void TryPaint(Vector2 uv)
    {
        if (Time.time - lastStrokeTime < strokeInterval)
            return;

        lastStrokeTime = Time.time;
        PaintAtUV(uv);

        int brushPx = Mathf.RoundToInt(brushSize * activeMask.width);

        if (analyzer.IsStampRevealed(
            activeMask,
            uv,
            brushPx
        ))
        {
            scratchManager.AdvanceFrame();
        }
    }

    void PaintAtUV(Vector2 uv)
    {
        paintMat.SetVector("_UV", uv);
        paintMat.SetFloat("_Size", brushSize);
        paintMat.SetColor("_Color", paintColor);
        paintMat.SetTexture("_Brush", brushTexture);

        RenderTexture temp = RenderTexture.GetTemporary(
            activeMask.width,
            activeMask.height,
            0,
            activeMask.format
        );

        Graphics.Blit(activeMask, temp);
        Graphics.Blit(temp, activeMask, paintMat);

        RenderTexture.ReleaseTemporary(temp);
    }
}
