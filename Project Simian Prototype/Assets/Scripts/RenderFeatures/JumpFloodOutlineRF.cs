using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

[System.Serializable]
public class JumpFloodOutlineRF : ScriptableRendererFeature {
    [SerializeField] private Shader jumpFloodShader;
    [SerializeField] private Shader silhouetteShader;
    [SerializeField] private Shader outlineShader;

    private Material jumpFloodMat;
    private Material silhouetteMat;
    private Material outlineMat;
    
    private JumpFloodOutlinePass outlinePass;
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(outlinePass);
    }

    public override void Create() {
        jumpFloodMat = CoreUtils.CreateEngineMaterial(jumpFloodShader);
        silhouetteMat = CoreUtils.CreateEngineMaterial(silhouetteShader);
        outlineMat = CoreUtils.CreateEngineMaterial(outlineShader);
        
        outlinePass = new JumpFloodOutlinePass(jumpFloodMat,silhouetteMat,outlineMat);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
        
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Depth);
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Color);
        outlinePass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
    }

    protected override void Dispose(bool disposing) {
        CoreUtils.Destroy(jumpFloodMat);
        CoreUtils.Destroy(silhouetteMat);
        CoreUtils.Destroy(outlineMat);
        
        outlinePass.Dispose();
    }
}
