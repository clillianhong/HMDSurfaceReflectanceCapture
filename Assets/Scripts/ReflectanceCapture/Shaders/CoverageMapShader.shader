// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/CoverageMapShader"
{
     Properties {
         _Color ("Base Color", Color) = (1,1,1,1)  
        _MapTexture ("Map Tex", 2D) = "white" {}
        _MainTex ("Base (RGB)", 2D) = "white" {}
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

        uniform bool debug; 
        uniform float4 camPos;
        uniform float4 lightPos; 
        uniform float beta1; 
        uniform float beta2;
        static const float PI = 3.14159265f;
	    uniform sampler2D _MapTexture;
        uniform sampler2D _MainTex;
        float4 _MapTexture_ST;


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
            float4 ndcVec = mul(camNDCMat, _worldPoint);

            float3 ndc = (ndcVec / ndcVec.w).xyz; 
            
            float3 surfacePoint = (_worldPoint / _worldPoint.w).xyz;
            // float3 surfacePoint = _worldPoint.xyz; 
            o.pos = UnityObjectToClipPos(v.vertex);
            // o.uv = TRANSFORM_TEX(v.texcoord.xy, _MapTexture);
            o.uv = v.texcoord.xy;

            if (ndc.x <= 1 && ndc.x >= -1 && ndc.y <= 1 && ndc.y >= -1 && ndc.z <= 1 && ndc.z >= -1){
                float3 viewDir = normalize(camPos.xyz - surfacePoint);
                float3 lightDir = normalize(lightPos.xyz - surfacePoint);
                float3 halfVec = normalize(viewDir + lightDir);

                o.thetaS = 180. * acos(dot(halfVec, normalize(v.normal))) / PI;
            }else{
                o.thetaS = -1;
            }

            return o;
        }

        float4 frag (v2f i) : SV_Target
        {

            if(debug == 1){
                float4 texcol = tex2D(_MapTexture, i.uv);
                // float4 texcol2 = float4(i.uv.x, i.uv.y, 0, 1);
                // float4 texcol2 = float4(1,1, 0, 1);
                return texcol; 
            }else{
                float4 texcol = float4(0,0,0,1);
                // float4 texcol = tex2D(mapTexture, i.uv);
                // texcol.w = 1; //full 
                
                if (i.thetaS != -1){
                    if (i.thetaS <= beta1 ){
                        texcol.z += 1.0;
                    } 
                    if (i.thetaS > beta1 && i.thetaS < beta2)
                    {
                        texcol.y += 1.0;
                    }
                    if(i.thetaS >= beta2){
                        texcol.x += 1.0;
                    }
                }
                
                return texcol;
                // return tex2D(_MainTex, i.uv);
            }
            
        }
        
            ENDCG
        }
    }
    FallBack "Diffuse"
}
