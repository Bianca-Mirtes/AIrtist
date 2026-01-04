Shader "Unlit/MultiLayerScratch_DoubleFrame"
{
    Properties
    {
        _MainTex ("Current Frame", 2D) = "black" {}
        _NextTex ("Next Frame", 2D) = "black" {}

        _Mask ("Current Diff Mask", 2D) = "black" {}
        _ScratchMask ("Scratch Mask", 2D) = "black" {}

        _OverlayColor ("Overlay Color", Color) = (1,1,1,0.35)
        _OverlayStrength ("Overlay Strength", Range(0,1)) = 1
        _DiffThreshold ("Diff Threshold", Range(0,0.2)) = 0.05

        _HasFrames ("Has Frames", Float) = 0
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

            sampler2D _MainTex;
            sampler2D _NextTex;

            sampler2D _Mask;
            sampler2D _ScratchMask;

            fixed4 _OverlayColor;
            float _OverlayStrength;
            float _DiffThreshold;

            float _HasFrames;

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

            fixed4 frag (v2f i) : SV_Target
            {
                if (_HasFrames < 0.5)
                {
                    discard; // ou return 0;
                }

                // 🔹 Frames
                fixed4 colCurr = tex2D(_MainTex, i.uv);
                fixed4 colNext = tex2D(_NextTex, i.uv);

                // 🔹 Máscaras
                float scratch = tex2D(_ScratchMask, i.uv).r;
                float diffMask = tex2D(_Mask, i.uv).r;

                // 🔑 Revelação SOMENTE onde:
                // - usuário pintou
                // - existe diferença real
                float reveal = scratch * diffMask;

                // 🔹 Revelação do próximo frame
                fixed4 revealed = lerp(colCurr, colNext, reveal);

                // 🔹 Diferença real entre frames (robusta)
                float diff =
                    abs(colNext.r - colCurr.r) +
                    abs(colNext.g - colCurr.g) +
                    abs(colNext.b - colCurr.b);

                diff /= 3.0;

                float diffThresholded = step(_DiffThreshold, diff);

                // 🔹 Overlay apenas onde ainda NÃO foi revelado
                float overlayFactor =
                    diffThresholded *
                    (1.0 - reveal) *
                    _OverlayStrength;

                fixed4 overlay = _OverlayColor;
                overlay.a *= overlayFactor;

                return lerp(revealed, overlay, overlay.a);
            }
            ENDCG
        }
    }
}
