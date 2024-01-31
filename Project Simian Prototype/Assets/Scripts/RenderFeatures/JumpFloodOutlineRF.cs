using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class JumpFloodOutlineRF : ScriptableRendererFeature {
    [SerializeField] private Shader jumpFloodShader;
    [SerializeField] private Shader outlineShader;
    [SerializeField] private Shader compositeShader;

    private Material jumpFloodMat;
    private Material outlineMat;
    private Material compositeMat;
    
    private JumpFloodOutlinePass outlinePass;
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(outlinePass);
    }

    public override void Create() {
        jumpFloodMat = CoreUtils.CreateEngineMaterial(jumpFloodShader);
        outlineMat = CoreUtils.CreateEngineMaterial(outlineShader);
        compositeMat = CoreUtils.CreateEngineMaterial(compositeShader);
        
        outlinePass = new JumpFloodOutlinePass(jumpFloodMat,outlineMat,compositeMat);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
        
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Depth);
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Color);
        outlinePass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
    }

    protected override void Dispose(bool disposing) {
        CoreUtils.Destroy(jumpFloodMat);
        CoreUtils.Destroy(outlineMat);
        CoreUtils.Destroy(compositeMat);
        
        outlinePass.Dispose();
    }
}
