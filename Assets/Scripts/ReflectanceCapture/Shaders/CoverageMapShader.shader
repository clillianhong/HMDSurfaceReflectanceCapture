// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/CoverageMapShader"
{
     Properties {
         _Color ("Base Color", Color) = (1,1,1,1)  
        _ProjTex ("Proj Tex", 2D) = "white" {}
    }

   SubShader {
        Pass {
            Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}

            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members thetaS)
            #pragma exclude_renderers d3d11

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

        
        //matrices 
        
        uniform float4x4 camNDCMat; 

        uniform float4 camPos;
        uniform float4 lightPos; 
        uniform float beta1; 
        uniform float beta2;
        static const float PI = 3.14159265f;
	    uniform sampler2D mapTexture;
        float4 mapTexture_ST;


        struct v2f {
            float4 pos:SV_POSITION;
            float thetaS:ANGLE;
            float2 uv:TEXCOORD0;
        };


        v2f vert (appdata_base v)
        {
            v2f o;

            float4x4 modelMatrix = unity_ObjectToWorld;

            float4 _worldPoint = mul(modelMatrix, v.vertex); 
            float3 ndc = mul(camNDCMat, _worldPoint).xyz;
            
            float3 surfacePoint = _worldPoint.xyz;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord.xy, mapTexture);

            if (ndc.x <= 1 && ndc.x >= -1 && ndc.y <= 1 && ndc.y >= -1 && ndc.z <= 1 && ndc.z >= -1){
                 float3 viewDir = normalize(camPos - surfacePoint).xyz;
                float3 lightDir = normalize(lightPos - surfacePoint).xyz;
                float3 halfVec = normalize(viewDir + lightDir).xyz;

                o.thetaS = 180 * acos(dot(halfVec, normalize(v.normal))) / PI;
            }else{
                o.thetaS = -1;
            }

            return o;
        }

        float4 frag (v2f i) : SV_Target
        {
            // float4 texcol = float4(0,0,0,1);
            float4 texcol = tex2D(mapTexture, i.uv);
            texcol.w = 1; //full 
            
            if (i.thetaS != -1){
                if (i.thetaS <= beta1 ){
                    texcol.z += 1;
                } 
                if (i.thetaS > beta1 && i.thetaS < beta2)
                {
                    texcol.y += 1;
                }
                if(i.thetaS >= beta2){
                    texcol.x += 1;
                }
            }
            
            return texcol;
        }
        
            ENDCG
        }
    }
    FallBack "Diffuse"
}
