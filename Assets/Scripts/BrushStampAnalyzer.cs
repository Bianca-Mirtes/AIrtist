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

        Texture2D diffMks = ConvertToR8(diffMask);

        var scratch = readback.GetRawTextureData<byte>();
        var diff = diffMks.GetRawTextureData<byte>();

        Debug.Log("Scratch: "+ scratch.Length);
        Debug.Log("diffMask: "+  diff.Length);

        int revealed = 0;
        int total = 0;

        for (int i = 0; i < diff.Length; i++)
        {
            bool allowed = diff[i] > 200;
            if (!allowed) continue;

            total++;

            if (scratch[i] > 200)
                revealed++;
        }

        return (float)revealed / total;
    }

    Texture2D ConvertToR8(Texture2D src)
    {
        Texture2D dst = new Texture2D(
            src.width,
            src.height,
            TextureFormat.R8,
            false,
            true
        );

        Color32[] pixels = src.GetPixels32();
        byte[] outData = new byte[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            // qualquer canal serve (R, G ou B)
            outData[i] = pixels[i].r;
        }

        dst.LoadRawTextureData(outData);
        dst.Apply(false, false);

        return dst;
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
