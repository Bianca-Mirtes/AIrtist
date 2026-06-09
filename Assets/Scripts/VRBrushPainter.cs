using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRBrushPainter : MonoBehaviour
{
    [Header("Brush Setup")]
    public Transform brushTip;
    public float maxDistance = 0.1f;
    public float brushSize = 0.02f;
    public Texture2D brushTexture;
    public Color paintColor = Color.white;

    [Header("Stroke Control")]
    public float strokeInterval = 0.03f;

    [Header("Scratch System")]
    public ScratchLayerManager scratchManager;

    List<InputDevice> devices = new();
    Material paintMat;
    public BrushStampAnalyzer analyzer;
    float lastStrokeTime;
    public bool canPaint = false;
    public bool advancedFrame = false;

    void Start()
    {
        paintMat = new Material(Shader.Find("Hidden/BrushPainter"));
        analyzer = new BrushStampAnalyzer();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (canPaint)
        {
            InputDeviceCharacteristics leftHandCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(leftHandCharacteristics, devices);
            if (devices.Count == 0)
                return;
            
            devices[0].TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);

            if (triggerValue > 0.2f)
            {
                Ray ray = new Ray(brushTip.position, brushTip.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
                {
                    TryPaint(hit.textureCoord);
                    float progress = analyzer.CalculateProgress(scratchManager.activeMask);

                    Debug.Log("Progress: " + progress);

                    if (progress >= 0.98f)
                    {
                        if (!advancedFrame)
                        {
                            scratchManager.AdvanceFrame();
                            advancedFrame = true;
                            Invoke("ResetAdvancedFrame", 1.5f);
                        }
                    }
                }
            }
        }
#else
        if (canPaint)
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
                    float progress = analyzer.CalculateProgress(scratchManager.activeMask);

                    Debug.Log("Progress: " + progress);

                    if (progress >= 0.98f)
                    {
                        if (!advancedFrame)
                        {
                            scratchManager.AdvanceFrame();
                            advancedFrame = true;
                            Invoke("ResetAdvancedFrame", 1.5f);
                        }
                    }
                }
            }
        }
#endif
    }

    void ResetAdvancedFrame()
    {
        advancedFrame = false;
    }

    void TryPaint(Vector2 uv)
    {
        if (Time.time - lastStrokeTime < strokeInterval)
            return;

        lastStrokeTime = Time.time;
        PaintAtUV(uv);
    }

    void PaintAtUV(Vector2 uv)
    {
        paintMat.SetVector("_UV", uv);
        paintMat.SetFloat("_Size", brushSize);
        paintMat.SetColor("_Color", paintColor);
        paintMat.SetTexture("_Brush", brushTexture);

        paintMat.SetVector(
            "_TexSize",
            new Vector4(
                scratchManager.activeMask.width,
                scratchManager.activeMask.height,
                0,
                0
            )
        );

        RenderTexture temp = RenderTexture.GetTemporary(
            scratchManager.activeMask.width,
            scratchManager.activeMask.height,
            0,
            scratchManager.activeMask.format
        );

        Graphics.Blit(scratchManager.activeMask, temp);
        Graphics.Blit(temp, scratchManager.activeMask, paintMat);

        RenderTexture.ReleaseTemporary(temp);
    }
}
