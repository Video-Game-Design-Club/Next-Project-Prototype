Shader "Unlit/OutlineTest"
{
    Properties {
        _BaseColor ("Color", Color) = (1, 1, 1)
        [Toggle] _UseOutlineNormals("Use Normals for Outline", Float) = 1
    }

    SubShader {

        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass {
            Tags{"LightMode" = "UniversalForward"}

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

            Interpolators vert(Attributes IN) {
                Interpolators OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            // The fragment shader definition.            
            float4 frag() : SV_Target {
                float4 customColor;
                customColor = float4(1, 1, 1, 1);
                return customColor;
            }
            ENDHLSL
        }

        Pass {
            Tags{"LightMode" = "OutlineEffect"}

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

            Interpolators vert(Attributes IN) {
                Interpolators OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            // The fragment shader definition.            
            float4 frag() : SV_Target {
                float4 customColor;
                customColor = float4(1, 1, 1, 1);
                return customColor;
            }
            ENDHLSL
        }

        Pass {
            Tags{"LightMode" = "DepthNormals"}

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

            Interpolators vert(Attributes IN) {
                Interpolators OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            // The fragment shader definition.            
            float4 frag() : SV_Target {
                float4 rightColor;
                rightColor = float4(-0.58,0.29,0.76,1);
                return lerp(rightColor,float4(0,1,0,1),_UseOutlineNormals);
            }
            ENDHLSL
        }
    }
}