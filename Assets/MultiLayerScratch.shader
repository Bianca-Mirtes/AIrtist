Shader "Unlit/MultiLayerScratch_Frames_2Arrays"
{
    Properties
    {
        _LayerA ("Layer A Frames", 2DArray) = "" {}
        _LayerB ("Layer B Frames", 2DArray) = "" {}
        _LayerA_DiffMasks("Masks Layer A", 2DArray) = "" {}
        _LayerB_DiffMasks("Masks Layer B", 2DArray) = "" {}

        _FrameIndex ("Frame Index", Int) = 0
        _UseLayerB ("Use Layer B", Int) = 0

        _ScratchMask ("Scratch Mask", 2D) = "black" {}
        _OverlayColor ("Overlay Color", Color) = (1,1,1,0.35)
        _OverlayStrength ("Overlay Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY(_LayerA);
            UNITY_DECLARE_TEX2DARRAY(_LayerB);
            UNITY_DECLARE_TEX2DARRAY(_LayerA_DiffMasks);
            UNITY_DECLARE_TEX2DARRAY(_LayerB_DiffMasks);

            sampler2D _ScratchMask;

            int _FrameIndex;
            int _UseLayerB;

            fixed4 _OverlayColor;
            float _OverlayStrength;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 SampleFrame(float2 uv, int frame)
            {
                if (_UseLayerB == 0)
                {
                    return UNITY_SAMPLE_TEX2DARRAY(
                        _LayerA,
                        float3(uv, frame)
                    );
                }
                else
                {
                    return UNITY_SAMPLE_TEX2DARRAY(
                        _LayerB,
                        float3(uv, frame)
                    );
                }
            }

            float SampleDiffMask(float2 uv, int frame)
            {
                if (_UseLayerB == 0)
                {
                    return UNITY_SAMPLE_TEX2DARRAY(
                        _LayerA_DiffMasks,
                        float3(uv, frame)
                    ).r;
                }
                else
                {
                    return UNITY_SAMPLE_TEX2DARRAY(
                        _LayerB_DiffMasks,
                        float3(uv, frame)
                    ).r;
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int curr = _FrameIndex;
                int next = curr + 1;

                fixed4 colCurr = SampleFrame(i.uv, curr);
                fixed4 colNext = SampleFrame(i.uv, next);

                float scratch = tex2D(_ScratchMask, i.uv).r;
                float d_Mask = SampleDiffMask(i.uv, curr);

                // 🔑 Revela SOMENTE onde:
                // - o usuário pintou
                // - o frame realmente muda
                float reveal = scratch * d_Mask;

                // 🔹 Revelação normal
                fixed4 revealed = lerp(colCurr, colNext, reveal);

                 // 🔹 DIFERENÇA REAL ENTRE FRAMES
                float diff =
                    abs(colNext.r - colCurr.r) +
                    abs(colNext.g - colCurr.g) +
                    abs(colNext.b - colCurr.b);

                diff /= 3.0; // normaliza

                // threshold pra evitar ruído mínimo
                float diffMask = step(0.05, diff);

                // overlay só onde:
                // - há diferença real
                // - ainda não foi revelado pela máscara
                float overlayFactor = diffMask * (1.0 - reveal) * _OverlayStrength;

                fixed4 overlay = _OverlayColor;
                overlay.a *= overlayFactor;

                return lerp(revealed, overlay, overlay.a);
                //return fixed4(diffMask, diffMask, diffMask, 1);
            }
            ENDCG
        }
    }
}
