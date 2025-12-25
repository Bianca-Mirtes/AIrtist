using UnityEngine;

public class BrushStampAnalyzer
{
    Texture2D readback;

    public float CalculateProgress(
        RenderTexture scratchRT,
        Texture2D diffMask
    )
    {
        EnsureReadback(scratchRT.width, scratchRT.height);

        var prev = RenderTexture.active;
        RenderTexture.active = scratchRT;

        readback.ReadPixels(
            new Rect(0, 0, scratchRT.width, scratchRT.height),
            0, 0
        );
        readback.Apply(false, false);

        RenderTexture.active = prev;

        var scratch = readback.GetRawTextureData<byte>();
        var diff = diffMask.GetRawTextureData<byte>();

        int revealed = 0;
        int total = 0;

        for (int i = 0; i < scratch.Length; i++)
        {
            bool allowed = diff[i] > 200;
            if (!allowed) continue;

            total++;

            if (scratch[i] > 200)
                revealed++;
        }

        return (float)revealed / total;
    }


    void EnsureReadback(int w, int h)
    {
        if (readback != null &&
            readback.width == w &&
            readback.height == h)
            return;

        if (readback != null)
            Object.Destroy(readback);

        readback = new Texture2D(
            w, h,
            TextureFormat.R8,
            false, true
        );
    }
}
