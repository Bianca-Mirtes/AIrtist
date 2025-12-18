Shader "Unlit/MultiLayerScratch_Validated"
{
    Properties
    {
        _Layer1 ("Layer 1 (Base)", 2D) = "white" {}
        _Layer2 ("Layer 2", 2D) = "white" {}
        _Layer3 ("Layer 3", 2D) = "white" {}

        _Mask1 ("Mask 1 (L1 -> L2)", 2D) = "black" {}
        _Mask2 ("Mask 2 (L2 -> L3)", 2D) = "black" {}

        _ValidityMask ("Validity Mask", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Layer1;
            sampler2D _Layer2;
            sampler2D _Layer3;

            sampler2D _Mask1;
            sampler2D _Mask2;

            sampler2D _ValidityMask;

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
                float valid = tex2D(_ValidityMask, i.uv).r;

                fixed4 col1 = tex2D(_Layer1, i.uv);
                fixed4 col2 = tex2D(_Layer2, i.uv);
                fixed4 col3 = tex2D(_Layer3, i.uv);

                if (valid < 0.5)
                    return col1; // nunca revela fora da área válida

                float m1 = tex2D(_Mask1, i.uv).r;
                float m2 = tex2D(_Mask2, i.uv).r;

                fixed4 result = lerp(col1, col2, m1);
                result = lerp(result, col3, m2);

                return result;
            }
            ENDCG
        }
    }
}
