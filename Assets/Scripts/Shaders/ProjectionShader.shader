// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ProjectionShader"
{
     Properties {
         _Color ("Base Color", Color) = (1,1,1,1)  
        _ProjTex ("Proj Tex", 2D) = "white" {}
    }

   SubShader {
        Pass {
            Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


        
        float4 _Color; 
	    sampler2D _ProjTex;
        uniform float4x4 projectM;

        struct v2f {
            float4 pos:SV_POSITION;
            float4 uv:TEXCOORD0;
        };


        v2f vert (appdata_base v)
        {
            v2f o;

            float4x4 modelMatrix = unity_ObjectToWorld;

            o.pos = UnityObjectToClipPos(v.vertex);
            // o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
            float4 proj_uv  = mul(mul(projectM, modelMatrix), v.vertex);
            // o.uv = float4((proj_uv.x + 1) / 2, (proj_uv.y + 1) / 2,( proj_uv.z + 1) / 2, proj_uv.w);
            // o.uv = (proj_uv + 1) / 2;
            o.uv = proj_uv;

            return o;
        }

        float4 frag (v2f i) : SV_Target
        {
            float4 texcol;
            
            if (i.uv.w > 0.0 ){
                float2 proj = i.uv.xy / i.uv.w;
                proj = (proj + 1) / 2; 
                texcol = tex2D(_ProjTex, proj); 
                // texcol = tex2Dproj(_ProjTex, i.uv);
            } 
            if (i.uv.w < 0)
            {
                texcol = float4(0,0,0,0);
            }
            
            return texcol;
        }
            ENDCG

        }
    }
    FallBack "Diffuse"
}
