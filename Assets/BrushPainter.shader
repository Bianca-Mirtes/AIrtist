Shader "Hidden/BrushPainter"
{
    Properties
    {
        _MainTex ("Base", 2D) = "black" {}
        _Brush ("Brush", 2D) = "white" {}
        _UV ("UV", Vector) = (0,0,0,0)
        _Size ("Size", Float) = 0.1
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Brush;
            float2 _UV;
            float _Size;
            float4 _Color;

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
                float2 d = i.uv - _UV;
                float dist = length(d);

                fixed4 baseCol = tex2D(_MainTex, i.uv);

                if (dist < _Size)
                {
                    float alpha = tex2D(_Brush, d / _Size + 0.5).a;
                    return max(baseCol, alpha * _Color);
                }

                return baseCol;
            }
            ENDCG
        }
    }
}
