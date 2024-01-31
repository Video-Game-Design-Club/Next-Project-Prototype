Shader "Unlit/JumpFloodProcess"
// Algo and original code: https://bgolus.medium.com/the-quest-for-very-wide-outlines-ba82ed442cd9
// Blitting Docs: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-features/how-to-fullscreen-blit.html
// Texture Sampling: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/writing-shaders-urp-unlit-texture.html
// Shader Graph Keywords: https://www.reddit.com/r/Unity3D/comments/ux23z4/how_to_add_tag_keyword_to_a_shader_using/#:~:text=In%20shader%20graph's%20blackboard%2C%20click,tag%20which%20removes%20the%20warning.
{
    Properties
    {
        //[MainTexture] _MainTex ("Texture", 2D) = "white" {}
        //_MainTex_TexelSize ("Texel Size", Vector) = (0,0,0,0) 
        //_StepWidth ("Step Width", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        
        HLSLINCLUDE
        // just inside the precision of a R16G16_SNorm to keep encoded range 1.0 >= and > -1.0
        #define SNORM16_MAX_FLOAT_MINUS_EPSILON ((float)(32768-2) / (float)(32768-1))
        #define FLOOD_ENCODE_OFFSET float2(1.0, SNORM16_MAX_FLOAT_MINUS_EPSILON)
        #define FLOOD_ENCODE_SCALE float2(2.0, 1.0 + SNORM16_MAX_FLOAT_MINUS_EPSILON)

        #define FLOOD_NULL_POS -1.0
        #define FLOOD_NULL_POS_FLOAT2 float2(FLOOD_NULL_POS, FLOOD_NULL_POS)
        ENDHLSL

        Pass {
            Name "JFA Initialize"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output structure (Varyings)
            // also the _BlitTexture thank you for telling me -_-
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            //#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/BlitColorAndDepth.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            #pragma vertex Vert
            #pragma fragment fragment

            //https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/writing-shaders-urp-unlit-texture.html
            
            float4 _TexelSize;
            int _StepWidth;

            float4 fragment (Varyings i) : SV_Target {
                // pixel position
                float2 uv = i.texcoord;

                // sample silhouette texture for sobel
                half3x3 values;
                //UNITY_UNROLL
                for(int u=0; u<3; u++)
                {
                    //UNITY_UNROLL
                    for(int v=0; v<3; v++)
                    {
                        float2 sampleUV = clamp(uv + float2((u-1)*_TexelSize.x, (v-1)*_TexelSize.y), int2(0,0), int2(1,1));
                        values[u][v] = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearRepeat, sampleUV).r;
                    }
                }
                // calculate output position for this pixel
                float2 outPos = uv * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET;

                int total = values._m00 + values._m01 + values._m02 + values._m10 + values._m11 + values._m12 + values._m20 + values._m21 + values._m22;
                if (total > 8.7)
                    return float4(FLOOD_NULL_POS_FLOAT2,0.0,1.0);

                // interior, return position
                if (values._m11 > 0.99)
                    return float4(outPos,0.0,1.0);

                // exterior, return no position
                if (values._m11 < 0.01)
                    return float4(FLOOD_NULL_POS_FLOAT2,0.0,1.0);

                // sobel to estimate edge direction
                float2 dir = -float2(
                    values[0][0] + values[0][1] * 2.0 + values[0][2] - values[2][0] - values[2][1] * 2.0 - values[2][2],
                    values[0][0] + values[1][0] * 2.0 + values[2][0] - values[0][2] - values[1][2] * 2.0 - values[2][2]
                    );

                // if dir length is small, this is either a sub pixel dot or line
                // no way to estimate sub pixel edge, so output position
                if (abs(dir.x) <= 0.005 && abs(dir.y) <= 0.005)
                    return float4(outPos,0.0,1.0);
                    //return float4(1.0,1.0,1.0,1.0);

                // normalize direction
                dir = normalize(dir);

                // sub pixel offset based on brightness of pixel
                float2 offset = dir * (1.0 - values._m11);

                // output encoded offset position
                float2 offPos = uv + (offset * abs(_TexelSize.xy)) * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET;
                return float4(offPos,0.0,1.0);
                //return float4(1.0,1.0,1.0,1.0);
            }
            
            ENDHLSL
        }

        Pass // 3
        {
            Name "JUMPFLOOD"

           HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output structure (Varyings)
            // also the _BlitTexture thank you for telling me -_-
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment fragment
            
            float4 _TexelSize;
            int _StepWidth;

            float4 fragment (Varyings i) : SV_Target {
                // pixel position
                float2 uv = i.texcoord;

                // initialize best distance at infinity
                float bestDist = 1.#INF;
                float2 bestCoord;

                //return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearRepeat, uv);

                // jump samples
                //UNITY_UNROLL
                for(int u=-1; u<=1; u++)
                {
                    //UNITY_UNROLL
                    for(int v=-1; v<=1; v++)
                    {
                        // calculate offset sample position
                        float2 offsetUV = uv + float2(u, v)*_TexelSize.xy * _StepWidth;

                        // clamp uvs
                        offsetUV = clamp(offsetUV, int2(0,0), int2(1,1));

                        // decode position from buffer
                        float2 raw = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearRepeat, offsetUV).rg;
                        float2 offsetPos = (raw + FLOOD_ENCODE_OFFSET) / FLOOD_ENCODE_SCALE;

                        // the offset from current position
                        float2 disp = uv - offsetPos;

                        // square distance
                        float dist = dot(disp, disp);

                        // if offset position isn't a null position or is closer than the best
                        // set as the new best and store the position
                        if (offsetPos.y != FLOOD_NULL_POS && dist < bestDist)
                        {
                            bestDist = dist;
                            bestCoord = offsetPos;
                        }
                    }
                }

                // if not valid best distance output null position, otherwise output encoded position
                return isinf(bestDist) ? float4(FLOOD_NULL_POS_FLOAT2,0.0,1.0) : float4(bestCoord * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET,0.0,1.0);
            }
            ENDHLSL
        }
    }
}