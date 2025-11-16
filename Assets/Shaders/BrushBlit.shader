Shader "Custom/BrushBlit"
{
    Properties
    {
        _PaintUV ("Paint UV", Vector) = (0,0,0,0)
        _PrevMask ("Previous Mask", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _PaintUV;          // x,y UV / z radius
            sampler2D _PrevMask;      // <-- BUILT-IN COMPATÍVEL
            
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(uint id : SV_VertexID)
            {
                v2f o;
                float2 verts[4] = {
                    float2(-1,-1), float2(-1,1),
                    float2(1,-1),  float2(1,1)
                };

                o.pos = float4(verts[id], 0, 1);
                o.uv = (verts[id] + 1) * 0.5;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // máscara anterior
                float prev = tex2D(_PrevMask, i.uv).r;

                // stroke do pincel
                float dist = distance(i.uv, _PaintUV.xy);
                float stroke = smoothstep(_PaintUV.z, 0, dist);

                // soma (acumula)
                float finalMask = max(prev, stroke);

                return float4(finalMask, 0, 0, 1);
            }
            ENDCG
        }
    }
}
