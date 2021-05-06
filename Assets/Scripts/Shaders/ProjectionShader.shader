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
        int multiMode; 
	    sampler2D _ProjTex1;
        sampler2D _ProjTex2;
        sampler2D _ProjTex3;
        uniform float4x4 projectM;
        uniform float4x4 projectM1;
        uniform float4x4 projectM2;
        uniform float4x4 projectM3;

        struct v2f {
            float4 pos:SV_POSITION;
            float4 uv:TEXCOORD0;
            float4 uv1:TEXCOORD1;
            float4 uv2:TEXCOORD2;
            float4 uv3:TEXCOORD3;
        };


        v2f vert (appdata_base v)
        {
            v2f o;

            float4x4 modelMatrix = unity_ObjectToWorld;

            o.pos = UnityObjectToClipPos(v.vertex);
            // o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
            o.uv  = mul(mul(projectM, modelMatrix), v.vertex);

            o.uv1  = mul(mul(projectM1, modelMatrix), v.vertex);
            o.uv2  = mul(mul(projectM2, modelMatrix), v.vertex);
            o.uv3  = mul(mul(projectM3, modelMatrix), v.vertex);
            // o.uv = float4((proj_uv.x + 1) / 2, (proj_uv.y + 1) / 2,( proj_uv.z + 1) / 2, proj_uv.w);
            // o.uv = (proj_uv + 1) / 2;
           
            return o;
        }

        float4 frag (v2f i) : SV_Target
        {
            float4 texcol;
            
            if (i.uv.w > 0.0 ){
               

                // if (multiMode==1){ //render 3 nearest neighbors at 1/3 opacity 
                    float2 proj1 = i.uv1.xy / i.uv1.w;
                    proj1 = (proj1 + 1) / 2; 
                    float4 tex1 = tex2D(_ProjTex1, proj1); 
                    tex1.w = 0.3;
                    float2 proj2 = i.uv2.xy / i.uv2.w;
                    proj2 = (proj2 + 1) / 2; 
                    float4 tex2 = tex2D(_ProjTex2, proj2); 
                    tex2.w = 0.3;
                    float2 proj3 = i.uv3.xy / i.uv3.w;
                    proj3 = (proj3+ 1) / 2; 
                    float4 tex3 = tex2D(_ProjTex3, proj3); 
                    tex3.w = 0.3; 
                    texcol = tex1 + tex2 + tex3;
                // }
                
                // else{ //render the nearest neighbor 
                //     float2 proj = i.uv.xy / i.uv.w;
                //     proj = (proj + 1) / 2; 
                //     texcol = tex2D(_ProjTex1, proj); 
                // }
               
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
