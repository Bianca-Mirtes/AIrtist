Shader "Custom/PaintingReveal_MultiArray"
{
    Properties
    {
        _Arr0 ("Frames Array 0", 2DArray) = "" {}
        _Arr1 ("Frames Array 1", 2DArray) = "" {}
        _Arr2 ("Frames Array 2", 2DArray) = "" {}
        _Arr3 ("Frames Array 3", 2DArray) = "" {}

        _BatchSize ("Batch Size", Int) = 100
        _TotalFrames ("Total Frames", Int) = 400

        _GlobalFrame ("Global Frame Index (float)", Float) = 0
        _Blend ("Blend", Range(0,1)) = 0

        _BrushNoise ("Brush Noise", 2D) = "white" {}
        _PaintMask ("Paint Mask", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // --- Texture2DArray declarations (Built-in macros)
            UNITY_DECLARE_TEX2DARRAY(_Arr0);
            UNITY_DECLARE_TEX2DARRAY(_Arr1);
            UNITY_DECLARE_TEX2DARRAY(_Arr2);
            UNITY_DECLARE_TEX2DARRAY(_Arr3);

            sampler2D _BrushNoise;
            sampler2D _PaintMask;

            int _BatchSize;
            int _TotalFrames;

            float _GlobalFrame;
            float _Blend;
            float _NoiseScale;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Helper: clamp int in HLSL (float-based)
            int clampInt(int x, int lo, int hi)
            {
                if (x < lo) return lo;
                if (x > hi) return hi;
                return x;
            }

            // Sample a global frame index across up to 5 arrays
            fixed4 SampleGlobalFrame(int globalIndex, float2 uv)
            {
                // clamp global index into valid range
                globalIndex = clampInt(globalIndex, 0, _TotalFrames - 1);

                int arrId = globalIndex / _BatchSize;        // which array
                int idxIn = globalIndex - arrId * _BatchSize; // layer inside that array

                // Safety: clamp arrId to [0,4]
                if (arrId < 0) arrId = 0;
                if (arrId > 3) arrId = 3;

                // Sample depending on arrId
                float3 uvw = float3(uv, idxIn);

                if (arrId == 0)
                    return UNITY_SAMPLE_TEX2DARRAY(_Arr0, uvw);

                if (arrId == 1)
                    return UNITY_SAMPLE_TEX2DARRAY(_Arr1, uvw);

                if (arrId == 2)
                    return UNITY_SAMPLE_TEX2DARRAY(_Arr2, uvw);

                if (arrId == 3)
                    return UNITY_SAMPLE_TEX2DARRAY(_Arr3, uvw);

                // default -> arr4
                return UNITY_SAMPLE_TEX2DARRAY(_Arr0, uvw);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate frame indices
                float framePos = saturate(_GlobalFrame);
                // but _GlobalFrame is expected to be absolute 0..(N-1), ensure floor works
                int idxA = (int)floor(_GlobalFrame);
                int idxB = idxA + 1;

                // clamp to total frames
                idxA = clampInt(idxA, 0, _TotalFrames - 1);
                idxB = clampInt(idxB, 0, _TotalFrames - 1);

                // Sample color from arrays
                fixed4 colA = SampleGlobalFrame(idxA, i.uv).rgba;
                fixed4 colB = SampleGlobalFrame(idxB, i.uv).rgba;

                // noise and mask
                float noise = tex2D(_BrushNoise, i.uv * _NoiseScale).r;
                float maskVal = tex2D(_PaintMask, i.uv).r;

                float t = saturate(_Blend * noise);

                // final mix
                fixed4 outCol = lerp(colA, colB, t * maskVal);

                return outCol;
            }
            ENDCG
        }
    }
}
