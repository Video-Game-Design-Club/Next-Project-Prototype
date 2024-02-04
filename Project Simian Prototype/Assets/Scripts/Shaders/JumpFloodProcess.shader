Shader "Hidden/JumpFloodProcess"
// Algo and original code: https://bgolus.medium.com/the-quest-for-very-wide-outlines-ba82ed442cd9
// Blitting Docs: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-features/how-to-fullscreen-blit.html
// Texture Sampling: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/writing-shaders-urp-unlit-texture.html
// Shader Graph Keywords: https://www.reddit.com/r/Unity3D/comments/ux23z4/how_to_add_tag_keyword_to_a_shader_using/#:~:text=In%20shader%20graph's%20blackboard%2C%20click,tag%20which%20removes%20the%20warning.

//This shader goes back and forth between pixel coordinates and UV coordinates a lot, be careful!
//Pixel coordinates are needed so that outlines are a constant width at all aspect rations
//UV coordinates are needed so that the frame debugger is still useful. Its all we've got, its important!
{

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        
        HLSLINCLUDE
        #define FLOOD_NULL_POS -10000.0
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

            #pragma vertex Vert
            #pragma fragment fragment

            //https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/writing-shaders-urp-unlit-texture.html
            
            float4 _TexelSize;
            int _StepWidth;

            float _minOutlineWidthSilhouette;
            float _maxOutlineWidthSilhouette;
            float _nearShrinkDistanceSilhouette;
            float _farShrinkDistanceSilhouette;

            TEXTURE2D(_DepthMap);
            SAMPLER(sampler_DepthMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _DepthMap_ST;
            CBUFFER_END

            float4 fragment (Varyings i) : SV_Target {
                float2 pos = i.texcoord * _TexelSize.zw;

                // sample silhouette texture for sobel
                half3x3 values;
                values [1][1] = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, i.texcoord).r;
                float depth = SAMPLE_TEXTURE2D(_DepthMap, sampler_DepthMap, i.texcoord);

                //calculate percentage between min & max outline width we're at
                float fac = clamp(depth,_farShrinkDistanceSilhouette,_nearShrinkDistanceSilhouette);
                fac = (fac - _farShrinkDistanceSilhouette) / (_nearShrinkDistanceSilhouette - _farShrinkDistanceSilhouette);

                //encode outline width as a weight in the blue channel
                float weight = lerp(_minOutlineWidthSilhouette,_maxOutlineWidthSilhouette, fac);
                
                bool border = false;
                //For overlapping objects, I only want to draw the outline for the object in the foreground
                //So only the foreground object's outline is rendered
                //I only want to check the neighboring pixels for depth once for performance, so this keeps track of that
                //This bit of logic became unnecessary now that I have line weights, but I wanna keep it for now just in case...
                //bool depthChecked = false;
                UNITY_UNROLL //unrolling loops helps with performance
                for(int u=0; u<3; u++)
                {
                    UNITY_UNROLL
                    for(int v=0; v<3; v++)
                    {
                        if (v == 1 && u == 1)
                            continue;
                        
                        float2 sampleUV = i.texcoord + (float2(u-1,v-1)* _TexelSize.xy);
                        values[u][v] = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, sampleUV).r;

                        //branchless way to say if a neighboring pixel has a different brightness, this is a border pixel
                        border = border || (values[u][v] != values[1][1]);

                        //this is the overlapping border removal code that I dont want to remove just yet
                        /*if (border && !depthChecked)
                        {
                            depthChecked = true;
                            float depthHere = SAMPLE_TEXTURE2D(_DepthMap, sampler_DepthMap, sampleUV);
                            if (depth < depthHere)
                                return float4(FLOOD_NULL_POS_FLOAT2,0.0,1.0);
                        }*/
                    }
                }

                //return float4(depth,depth,depth,1.0);

                // interior, return no position
                //int total = values._m00 + values._m01 + values._m02 + values._m10 + values._m11 + values._m12 + values._m20 + values._m21 + values._m22;
                if (!border)
                    return float4(FLOOD_NULL_POS_FLOAT2,0.0,1.0);

                // exterior, return no position
                if (values._m11 < 0.01)
                    return float4(FLOOD_NULL_POS_FLOAT2,0.0,1.0);

                // no aliasing, return position
                //if (values._m11 > 0.99)
                    return float4(i.texcoord,weight,1.0);

                // this code below assumes the silhouette buffer objects are rendered at full brightness
                // this is no longer the case, since I want to have overlapping object outlines
                // there may be a way to fix it, but I haven't thought of a way yet.
                // the commented if statement above means the code always returns there, and the code below never runs
                
                // sobel to estimate edge direction for anti aliasing
                float2 dir = -float2(
                    values[0][0] + values[0][1] * 2.0 + values[0][2] - values[2][0] - values[2][1] * 2.0 - values[2][2],
                    values[0][0] + values[1][0] * 2.0 + values[2][0] - values[0][2] - values[1][2] * 2.0 - values[2][2]
                    );

                // if dir length is small, this is either a sub pixel dot or line
                // no way to estimate sub pixel edge, so output position
                if (abs(dir.x) <= 0.005 && abs(dir.y) <= 0.005)
                    return float4(i.texcoord,0.0,1.0);

                // normalize direction
                dir = normalize(dir);

                // sub pixel offset based on brightness of pixel
                float2 offset = dir * (1.0 - values._m11);

                // output encoded offset position
                float2 offPos = (pos + offset) * abs(_TexelSize.xy);
                return float4(offPos,0.0,1.0);
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
                float2 pos = i.texcoord * _TexelSize.zw;

                // initialize best distance at infinity
                float bestDist = 1.#INF;
                float2 bestCoord;
                float bestWeight = -1;

                // jump samples
                UNITY_UNROLL
                for(int u=-1; u<=1; u++)
                {
                    UNITY_UNROLL
                    for(int v=-1; v<=1; v++)
                    {
                        // calculate offset sample position
                        float2 offsetPos = pos + float2(u, v) * _StepWidth;

                        // decode position from buffer
                        float3 nearestSample = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, offsetPos * _TexelSize.xy).rgb;
                        float2 nearestCoord = nearestSample.rg;
                        float2 nearestPos = nearestCoord * _TexelSize.zw;
                        float nearestWeight = nearestSample.b;

                        // the offset from current position
                        float2 disp = pos - nearestPos;

                        // square distance
                        float dist = dot(disp, disp);
                        //offset distance by the weight, so higher weighted positions evaluate closer than they appear
                        dist = dist - (nearestWeight*nearestWeight/4);//I dont know why dividing by 4 makes it work but it does

                        // if offset position isn't a null position or is closer than the best
                        // set as the new best and store the position
                        if (nearestPos.y != FLOOD_NULL_POS && dist < bestDist )
                        {
                            bestDist = dist;
                            bestCoord = nearestCoord;
                            bestWeight = nearestWeight;
                        }
                    }
                }

                // if not valid best distance output null position, otherwise output encoded position
                return isinf(bestDist) ? float4(FLOOD_NULL_POS_FLOAT2,0.0,1.0) : float4(bestCoord,bestWeight,1.0);
            }
            ENDHLSL
        }
    }
}