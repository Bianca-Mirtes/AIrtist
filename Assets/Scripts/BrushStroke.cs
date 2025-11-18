using UnityEngine;

[System.Serializable]
public class BrushStroke
{
    public int pairIndex;
    public string imgFrom;
    public string imgTo;

    public Vector2 uvStart;
    public Vector2 uvEnd;

    public Vector2 centroidUV;

    public Vector2 directionUV;
    public float lengthUV;

    public float angleRad;
    public float sizePixels;

    // Frames correspondentes
    public int frameFrom;
    public int frameTo;
}