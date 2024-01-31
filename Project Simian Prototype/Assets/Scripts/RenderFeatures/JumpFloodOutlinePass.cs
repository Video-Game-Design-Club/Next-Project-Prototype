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

    private RenderTextureDescriptor cameraDescriptor;

    private RTHandle cameraColorRTHandle;
    private RTHandle cameraDepthRTHandle;

    private RTHandle jfaBuffer1;
    private RTHandle jfaBuffer2;
    private RTHandle silhouetteBuffer;
    private RTHandle silhouetteDepthBuffer;
    private int jfaBuffer1ID;
    private int jfaBuffer2ID;
    private int silBufferID;
    private int silDepthBufferID;

    private uint renderingLayerMask = 256;

    public JumpFloodOutlinePass(Material jumpFloodMat, Material silhouetteMat, Material outlineMat) {
        this.jumpFloodMat = jumpFloodMat;
        this.silhouetteMat = silhouetteMat;
        this.outlineMat = outlineMat;

        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        jfaBuffer1ID = Shader.PropertyToID("_JFABuffer1");
        jfaBuffer2ID = Shader.PropertyToID("_JFABuffer2");
        silBufferID = Shader.PropertyToID("_SilhouetteBuffer");
        silDepthBufferID = Shader.PropertyToID("_SilhouetteDepthBuffer");
        jfaBuffer1 = RTHandles.Alloc(jfaBuffer1ID, "JFA Buffer 1");
        jfaBuffer2 = RTHandles.Alloc(jfaBuffer2ID, "JFA Buffer 2");
        silhouetteBuffer = RTHandles.Alloc(silBufferID, "Silhouette Buffer");
        silhouetteDepthBuffer = RTHandles.Alloc(silDepthBufferID, "Silhouette Depth Buffer");

        //https://www.youtube.com/watch?v=9fa4uFm1eCE&list=PLq2tY5pRDWwh853rmZmVzXfL8yWZx-iPk&index=10
        //12:35 for setting texture type maybe??
        //textureFormat=??
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        //set material properties here if needed
        var descBuff = cameraDescriptor;
        descBuff.depthBufferBits = (int)DepthBits.None;
        descBuff.msaaSamples = 1;
        descBuff.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
        RenderingUtils.ReAllocateIfNeeded(ref jfaBuffer1, descBuff, FilterMode.Point,
            TextureWrapMode.Clamp); //more options here
        RenderingUtils.ReAllocateIfNeeded(ref jfaBuffer2, descBuff, FilterMode.Point, TextureWrapMode.Clamp);

        var descSil = cameraDescriptor;
        descSil.depthBufferBits = (int)DepthBits.None;
        descSil.msaaSamples = Mathf.Max(1, QualitySettings.antiAliasing);
        descSil.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;
        RenderingUtils.ReAllocateIfNeeded(ref silhouetteBuffer, descSil, FilterMode.Point, TextureWrapMode.Clamp);

        var descDepth = cameraDescriptor;
        descDepth.depthBufferBits = (int)DepthBits.Depth32;
        descDepth.msaaSamples = Mathf.Max(1, QualitySettings.antiAliasing);
        descDepth.graphicsFormat = GraphicsFormat.D32_SFloat;
        RenderingUtils.ReAllocateIfNeeded(ref silhouetteDepthBuffer, descDepth, FilterMode.Point,
            TextureWrapMode.Clamp);


        int width = silhouetteBuffer.GetScaledSize().x;
        int height = silhouetteBuffer.GetScaledSize().y;
        jumpFloodMat.SetVector("_TexelSize", new Vector4(1.0f / width, 1.0f / height, width, height));


        CommandBuffer cmd1 = CommandBufferPool.Get();

        using (new ProfilingScope(cmd1, new ProfilingSampler("JFA Silhouette Pass"))) {
            cmd1.SetRenderTarget(silhouetteBuffer, silhouetteDepthBuffer);
            cmd1.ClearRenderTarget(true, true, Color.black);
            context.ExecuteCommandBuffer(cmd1);
            cmd1.Clear();
            CommandBufferPool.Release(cmd1);

            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings =
                CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = silhouetteMat;
            FilteringSettings filteringSettings = new FilteringSettings(null, -1,renderingLayerMask);

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            //Blitter.BlitCameraTexture(cmd,cameraColorRTHandle,jfaBuffer1,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store,jumpFloodMat,0);
            //cmd.SetGlobalTexture("_textname",jfaBuffer1ID);
        }

        CommandBuffer cmd2 = CommandBufferPool.Get();
        using (new ProfilingScope(cmd2, new ProfilingSampler("Jump Flooding!"))) {
            // choose a starting buffer so we always finish on the same buffer
            int jfaPasses = 8;
            RTHandle startBuffer = (jfaPasses % 2 == 0) ? jfaBuffer2 : jfaBuffer1;

            // jfa init
            Blitter.BlitCameraTexture(cmd2, silhouetteBuffer, startBuffer, RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store, jumpFloodMat, 0);

            // jfa flood passes
            for (int i=jfaPasses; i>=0; i--)
            {
                // calculate appropriate jump width for each iteration
                // + 0.5 is just me (blog guy) being cautious to avoid any floating point math rounding errors
                //jumpFloodMat.SetFloat("_StepWidth", Mathf.FloorToInt(Mathf.Pow(2, i)+0.5f));
                cmd2.SetGlobalFloat("_StepWidth", (Mathf.Pow(2, i)+0.5f));

                // ping pong between buffers
                if (i % 2 == 1)
                    Blitter.BlitCameraTexture(cmd2, jfaBuffer1, jfaBuffer2, RenderBufferLoadAction.DontCare,
                        RenderBufferStoreAction.Store, jumpFloodMat, 1);
                else
                    Blitter.BlitCameraTexture(cmd2, jfaBuffer2, jfaBuffer1, RenderBufferLoadAction.DontCare,
                        RenderBufferStoreAction.Store, jumpFloodMat, 1);
            }
            
            

            context.ExecuteCommandBuffer(cmd2);
            cmd2.Clear();
            CommandBufferPool.Release(cmd2);
        }
    }

    public void SetTarget(RTHandle cameraColorRTHandle, RTHandle cameraDepthRTHandle) {
        this.cameraColorRTHandle = cameraColorRTHandle;
        this.cameraDepthRTHandle = cameraDepthRTHandle;
    }

    public override void OnCameraCleanup(CommandBuffer cmd) {
        cameraColorRTHandle = null;
        cameraDepthRTHandle = null;
    }

    public void Dispose() {
        jfaBuffer1.Release();
        jfaBuffer2.Release();
        silhouetteBuffer.Release();
        silhouetteDepthBuffer.Release();
    }
}