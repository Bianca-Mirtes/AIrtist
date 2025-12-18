Shader "Unlit/MultiLayerScratch_Frames_2Arrays"
{
    Properties
    {
        _LayerA ("Layer A Frames", 2DArray) = "" {}
        _LayerB ("Layer B Frames", 2DArray) = "" {}

        _FrameIndex ("Frame Index", Int) = 0
        _UseLayerB ("Use Layer B", Int) = 0

        _ScratchMask ("Scratch Mask", 2D) = "black" {}
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

            sampler2D _ScratchMask;

            int _FrameIndex;
            int _UseLayerB;

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

            fixed4 frag (v2f i) : SV_Target
            {
                int curr = _FrameIndex;
                int next = curr + 1;

                fixed4 colCurr = SampleFrame(i.uv, curr);
                fixed4 colNext = SampleFrame(i.uv, next);

                float mask = tex2D(_ScratchMask, i.uv).r;

                // máscara revela SOMENTE o próximo frame
                return lerp(colCurr, colNext, mask);
            }
            ENDCG
        }
    }
}
