using UnityEngine;

public class BrushStampAnalyzer
{
    Texture2D readback;

    public bool IsStampRevealed(
        RenderTexture mask,
        Vector2 uv,
        int brushSizePx,
        float threshold = 0.98f
    )
    {
        if (readback == null ||
            readback.width != brushSizePx ||
            readback.height != brushSizePx)
        {
            if (readback != null)
                Object.Destroy(readback);

            readback = new Texture2D(
                brushSizePx,
                brushSizePx,
                TextureFormat.R8,
                false,
                true
            );
        }

        RenderTexture.active = mask;

        int x = Mathf.RoundToInt(uv.x * mask.width) - brushSizePx / 2;
        int y = Mathf.RoundToInt(uv.y * mask.height) - brushSizePx / 2;

        x = Mathf.Clamp(x, 0, mask.width - brushSizePx);
        y = Mathf.Clamp(y, 0, mask.height - brushSizePx);

        readback.ReadPixels(
            new Rect(x, y, brushSizePx, brushSizePx),
            0,
            0,
            false
        );

        readback.Apply(false, false);

        var pixels = readback.GetRawTextureData<byte>();

        int revealed = 0;
        int total = pixels.Length;

        for (int i = 0; i < total; i++)
        {
            if (pixels[i] > 200)
                revealed++;
        }

        float ratio = revealed / (float)total;
        return ratio >= threshold;
    }
}
