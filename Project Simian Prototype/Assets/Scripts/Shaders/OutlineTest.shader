Shader "Unlit/OutlineTest"

//I tried to make this in shader graph, but its just too complex for its own good
//It kept casting very dim shadows from objects I'm not even rendering and ruining the effect
//So sorry but this one is a proper shader :/
{
    Properties {
        _BaseColor ("Color", Color) = (1, 1, 1)
    }

    SubShader {

        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "DisableBatching"="True"}

        Pass {
            Tags{"LightMode" = "UniversalForward" "DisableBatching"="True"}

            ZTest LEqual
            // ZTest Less | Greater | GEqual | Equal | NotEqual | Always
            
            ZWrite On		// Default
            // ZWrite Off

            //Blend SrcAlpha OneMinusSrcAlpha

            Cull Off

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 

            float4 _BaseColor;   
            float _UseOutlineNormals;        

            struct Attributes {
                float4 positionOS   : POSITION;                 
            };

            struct Interpolators {
                float4 positionHCS  : SV_POSITION;
            };

            //https://gist.github.com/keijiro/24f9d505fac238c9a2982c0d6911d8e3
            //https://thebookofshaders.com/11/
            //this can be better im just exhausted rn
            float random (in float2 st) {
                return frac(sin(dot(st.xy,
                                     float2(12.9898,78.233)))
                             * 43758.5453123);
            }

            Interpolators vert(Attributes IN) {
                Interpolators OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            // The fragment shader definition.            
            float4 frag() : SV_Target {
                float3 pos = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;//unity_ObjectToWorld._m03_m13_m23;
                pos = pos * float3(12807.29084,90458.309485,390475.903475) + float3(329074.9023,9038.239,2309.293);
                float rand = random(float2(pos.x+pos.z,pos.y+pos.z));
                rand = (rand*0.9)+0.1; //from 0.1 -> 1 hopefully
                return float4(rand,0,0,1);
            }
            ENDHLSL
        }
    }
}