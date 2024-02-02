using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

//Renderer Feature API Help:
//Walkthrough of Basics: https://www.youtube.com/watch?v=9fa4uFm1eCE&list=PLq2tY5pRDWwh853rmZmVzXfL8yWZx-iPk&index=10
//Unity's Tutorial: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-features/create-custom-renderer-feature.html
//Blitting Overview: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/customize/blit-overview.html
//DrawRenderers Example: https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.DrawRenderers.html

//URP Despair Thread (also shows hecked up screen UV coords): https://forum.unity.com/threads/urp-13-1-8-proper-rthandle-usage-in-a-renderer-feature.1341164/

[System.Serializable]
public class JumpFloodOutlineRF : ScriptableRendererFeature {
    [SerializeField] private JumpFloodOutlineSettings settings = new JumpFloodOutlineSettings();

    [System.Serializable]
    public class JumpFloodOutlineSettings {
        [HideInInspector]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        
        public Shader jumpFloodShader;
        public Shader silhouetteShader;
        public Shader outlineShader;
        
        public float outlineWidth = 5f;
    }
    
    private JumpFloodOutlinePass outlinePass;
    
    //Add Render Pass to Pipeline
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(outlinePass);
    }

    //Setup Materials and Create Our Pass
    public override void Create() {
        outlinePass = new JumpFloodOutlinePass(settings);
    }

    //Give Our Pass References to the Camera's Color and Depth Textures
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Depth);
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Color);
        outlinePass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
    }

    //Free Render Textures & Materials from Memory. VERY IMPORTANT!!!
    protected override void Dispose(bool disposing) {
        outlinePass.Dispose();
    }
}
