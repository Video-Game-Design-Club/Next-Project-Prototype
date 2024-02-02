using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class JumpFloodOutlinePass : ScriptableRenderPass {
    private Material jumpFloodMat;
    private Material silhouetteMat;
    private Material outlineMat;

    private float outlineWidth;

    private RenderTextureDescriptor cameraDescriptor; //camera rendering settings
    private RTHandle cameraColorRTHandle; //camera color buffer
    private RTHandle cameraDepthRTHandle; //camera depth buffer

    private RTHandle jfaBuffer1; //jump flood works by ping-ponging between 2 render textures
    private RTHandle jfaBuffer2;
    private RTHandle silhouetteBuffer;
    private RTHandle silhouetteDepthBuffer;

    private uint renderingLayerMask = 256; //mask for render layer 8, which I've marked as the outline layer

    public JumpFloodOutlinePass(JumpFloodOutlineRF.JumpFloodOutlineSettings settings) {
        jumpFloodMat = CoreUtils.CreateEngineMaterial(settings.jumpFloodShader);
        silhouetteMat = CoreUtils.CreateEngineMaterial(settings.silhouetteShader);
        outlineMat = CoreUtils.CreateEngineMaterial(settings.outlineShader);

        renderPassEvent = settings.renderPassEvent;
        outlineWidth = settings.outlineWidth;
    }

    //save camera settings for later
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        
        //setup both jump flood blit textures aaa
        var descBuff = cameraDescriptor;
        descBuff.depthBufferBits = (int)DepthBits.None;
        descBuff.msaaSamples = 1;
        descBuff.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
        RenderingUtils.ReAllocateIfNeeded(ref jfaBuffer1, descBuff, FilterMode.Point,
            TextureWrapMode.Clamp); //more options here
        RenderingUtils.ReAllocateIfNeeded(ref jfaBuffer2, descBuff, FilterMode.Point, TextureWrapMode.Clamp);

        //setup silhouette render texture
        var descSil = cameraDescriptor;
        descSil.depthBufferBits = (int)DepthBits.None;
        descSil.msaaSamples = Mathf.Max(1, QualitySettings.antiAliasing);
        descSil.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;
        RenderingUtils.ReAllocateIfNeeded(ref silhouetteBuffer, descSil, FilterMode.Point, TextureWrapMode.Clamp);

        //setup silhouette depth texture. Without this, .DrawRenderers() will not sort depth at all and everything breaks
        var descDepth = cameraDescriptor;
        descDepth.depthBufferBits = (int)DepthBits.Depth32;
        descDepth.msaaSamples = Mathf.Max(1, QualitySettings.antiAliasing);
        descDepth.graphicsFormat = GraphicsFormat.D32_SFloat;
        RenderingUtils.ReAllocateIfNeeded(ref silhouetteDepthBuffer, descDepth, FilterMode.Point,
            TextureWrapMode.Clamp);
    }

    //this is where the magic happens!
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        
        //send screen size to the jump flood shader
        //there was a built-in shader include that used to provide this, but I don't know what it is for URP and I can't find it :(
        int width = silhouetteBuffer.GetScaledSize().x;
        int height = silhouetteBuffer.GetScaledSize().y;
        jumpFloodMat.SetVector("_TexelSize", new Vector4(1.0f / width, 1.0f / height, width, height));
        
        //scale outline width with screen size
        float outlineWidthScaled = outlineWidth * (Mathf.Min(width, height) / 1080f);
        //calculate number of jump flood iterations needed
        int jfaPasses = Mathf.CeilToInt(Mathf.Log(outlineWidthScaled * 0.5f + 1.0f, 2f));
        jfaPasses = Mathf.Clamp(jfaPasses, 1, 20);

        //command buffers store graphics commands and execute them all at once in the .ExecuteCommandBuffer() call
        //I think it helps with optimization knowing what commands come later, so this is how its done
        //Just remember, any cmd function is not executed on the line you type it!!! It happens all at once later!!!
        CommandBuffer cmd1 = CommandBufferPool.Get();

        //draw silhouette of all objects on the Outline render layer to a render texture
        //profiling scopes just organize events in the frame debugger, they don't actually do anything btw
        using (new ProfilingScope(cmd1, new ProfilingSampler("JFA Silhouette Pass"))) {
            //set our texture as the render destination, instead of the screen
            cmd1.SetRenderTarget(silhouetteBuffer, silhouetteDepthBuffer);
            //set the texture to all black just in case
            cmd1.ClearRenderTarget(true, true, Color.black);
            //run our commands
            context.ExecuteCommandBuffer(cmd1);
            //empty the command buffer and return it.
            cmd1.Clear();
            CommandBufferPool.Release(cmd1);

            //culling data from the camera needed for rendering
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            //replace all object's own materials with the silhouette rendering shader
            DrawingSettings drawingSettings =
                CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = silhouetteMat;
            //filter out any objects that aren't on the Outline rendering layer
            FilteringSettings filteringSettings = new FilteringSettings(null, -1, renderingLayerMask);

            //draw them renderers!
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        //execute the jump flood algorithm on the silhouette buffer
        CommandBuffer cmd2 = CommandBufferPool.Get();
        using (new ProfilingScope(cmd2, new ProfilingSampler("Jump Flooding!"))) {
            // choose one of the jump flood buffers so that we always end on jfaBuffer1
            //jfaPasses = 5;
            RTHandle startBuffer = (jfaPasses % 2 == 0) ? jfaBuffer2 : jfaBuffer1;

            // process silhouette buffer into data the jump flood process can use
            Blitter.BlitCameraTexture(cmd2, silhouetteBuffer, startBuffer, jumpFloodMat, 0);

            // render jump flood passes
            for (int i = jfaPasses; i >= 0; i--) {
                // calculate appropriate jump width for each iteration
                // this get rounded to an int, so the +0.5f makes sure nothing underflows when converting (probably not needed)
                // this needs to be done with a command buffer or else the timing won't work
                cmd2.SetGlobalFloat("_StepWidth", (Mathf.Pow(2, i) + 0.5f));

                // ping pong between buffers with decreasing step widths
                if (i % 2 == 1)
                    Blitter.BlitCameraTexture(cmd2, jfaBuffer1, jfaBuffer2, jumpFloodMat, 1);
                else
                    Blitter.BlitCameraTexture(cmd2, jfaBuffer2, jfaBuffer1, jumpFloodMat, 1);
            }
        }

        //draw the outline using the jump flood distance field!
        using (new ProfilingScope(cmd2, new ProfilingSampler("Draw Outline"))) {
            //set outline width
            outlineMat.SetFloat("_Outline_Width",outlineWidthScaled);
            //this if statement prevents null refs in the editor believe it or not
            if (cameraColorRTHandle != null) { 
                //blit from the distance field to the screen using the outline shader
                Blitter.BlitCameraTexture(cmd2, jfaBuffer1, cameraColorRTHandle, outlineMat, 0);
            }
            //execute commands!
            context.ExecuteCommandBuffer(cmd2);
            cmd2.Clear();
            CommandBufferPool.Release(cmd2);
        }
        //all done! :D
    }

    //gets references to camera buffers. This function name sucks, sorry!
    public void SetTarget(RTHandle cameraColorRTHandle, RTHandle cameraDepthRTHandle) {
        this.cameraColorRTHandle = cameraColorRTHandle;
        this.cameraDepthRTHandle = cameraDepthRTHandle;
    }

    //THIS PREVENTS A VERY STUPID BUT VERY BAD MEMORY LEAK
    public override void OnCameraCleanup(CommandBuffer cmd) {
        if (cmd == null) {//Unity's code does this so I put it here, idk why!
            throw new System.ArgumentNullException("cmd");
        }
        
        cameraColorRTHandle = null;
        cameraDepthRTHandle = null;
    }

    //frees render textures & materials from memory IF ONLY UNITY WOULD CALL IT D:<
    public void Dispose() {
        if (jumpFloodMat != null) {
            CoreUtils.Destroy(jumpFloodMat);
            CoreUtils.Destroy(silhouetteMat);
            CoreUtils.Destroy(outlineMat);
        }
        
        jfaBuffer1?.Release();
        jfaBuffer2?.Release();
        silhouetteBuffer?.Release();
        silhouetteDepthBuffer?.Release();
    }
}