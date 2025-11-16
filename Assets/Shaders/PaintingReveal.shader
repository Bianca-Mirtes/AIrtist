Shader "Custom/PaintingRevealArray_Builtin"
{
    Properties
    {
        _Frames ("Painting Frames (Texture2DArray)", 2DArray) = "" {}
        _FrameIndex ("Frame Index", Float) = 0
        _Blend ("Blend Between Frames", Range(0,1)) = 0
        _BrushNoise ("Brush Noise", 2D) = "white" {}
        _PaintMask ("Paint Mask", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5   // IMPORTANTE! Necessário para 2DArray

            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY(_Frames);
            sampler2D _BrushNoise;

            float _FrameIndex;
            float _Blend;
            float _NoiseScale;
            sampler2D _PaintMask;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                int idxA = floor(_FrameIndex);
                int idxB = idxA + 1;

                idxA = clamp(idxA, 0, 499);
                idxB = clamp(idxB, 0, 499);

                float3 uvA = float3(i.uv, idxA);
                float3 uvB = float3(i.uv, idxB);

                float4 colA = UNITY_SAMPLE_TEX2DARRAY(_Frames, uvA);
                float4 colB = UNITY_SAMPLE_TEX2DARRAY(_Frames, uvB);

                float noise = tex2D(_BrushNoise, i.uv * _NoiseScale).r;

                float maskVal = tex2D(_PaintMask, i.uv).r;

                float t = saturate(_Blend * noise);

                return lerp(colA, colB, t* maskVal);
            }
            ENDCG
        }
    }
}
