using UnityEngine;

public class BrushStampAnalyzer
{
    Texture2D readback;

    public bool IsStampValid(
      RenderTexture scratchMask,
      Texture2D diffMask,
      Vector2 uv,
      int brushSizePx,
      float threshold = 0.98f
  )
    {
        EnsureReadback(brushSizePx);

        int x = Mathf.RoundToInt(uv.x * scratchMask.width) - brushSizePx / 2;
        int y = Mathf.RoundToInt(uv.y * scratchMask.height) - brushSizePx / 2;

        x = Mathf.Clamp(x, 0, scratchMask.width - brushSizePx);
        y = Mathf.Clamp(y, 0, scratchMask.height - brushSizePx);

        // 🔹 Lê ScratchMask (GPU → CPU)
        RenderTexture.active = scratchMask;
        readback.ReadPixels(
            new Rect(x, y, brushSizePx, brushSizePx),
            0, 0, false
        );
        readback.Apply(false, false);

        var scratchPixels = readback.GetRawTextureData<byte>();
        var diffPixels = diffMask.GetRawTextureData<byte>();

        int valid = 0;
        int total = scratchPixels.Length;

        for (int i = 0; i < total; i++)
        {
            bool scratched = scratchPixels[i] > 200;
            bool allowed = diffPixels[i] > 200;

            if (scratched && allowed)
                valid++;
        }

        float ratio = valid / (float)total;
        return ratio >= threshold;
    }

    void EnsureReadback(int size)
    {
        if (readback != null &&
            readback.width == size &&
            readback.height == size)
            return;

        if (readback != null)
            Object.Destroy(readback);

        readback = new Texture2D(
            size, size,
            TextureFormat.R8,
            false, true
        );
    }
}
