using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    [Serializable]
    public class DeferredLitSettings
    {
        public Material mat;
        public int passIndex;
    }
    
    public class DeferredToonLitFeature : RoXamiRenderFeature
    {
        [SerializeField]
        Material deferredToonLitMaterial;
        
        [SerializeField]
        DeferredLitSettings[] deferredCustomLitSettings;

        GBufferPass gBufferPass;
        CopyCameraDepthPass copyDepthPass;
        ScreenSpaceShadowsPass ssShadowsPass;
        DeferredDiffusePass deferredDiffusePass;
        DeferredCustomPass deferredCustomPass;
        DeferredGiPass deferredGiPass;
        
        public override void Create()
        {
            gBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer + 10);

            copyDepthPass = new CopyCameraDepthPass(RenderPassEvent.BeforeRenderingGbuffer + 11);
            
            ssShadowsPass = new ScreenSpaceShadowsPass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 9);
            
            deferredDiffusePass = new DeferredDiffusePass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 10, deferredToonLitMaterial);
            
            if (deferredCustomLitSettings != null && deferredCustomLitSettings.Length != 0)
            {
                deferredCustomPass = new DeferredCustomPass(
                    RenderPassEvent.BeforeRenderingDeferredDiffuse + 11, 
                    deferredCustomLitSettings);
            }

            deferredGiPass = new DeferredGiPass(RenderPassEvent.BeforeRenderingDeferredGI + 10);
        }

        public override void AddRenderPasses(RoXamiRenderLoop renderLoop, ref RenderingData renderingData)
        {
            renderingData.runtimeData.isDeferred = true;
            
            renderLoop.EnqueuePass(gBufferPass);
            
            renderLoop.EnqueuePass(copyDepthPass);
            
            renderLoop.EnqueuePass(ssShadowsPass);
            
            renderLoop.EnqueuePass(deferredDiffusePass);
            
            if (deferredCustomLitSettings != null && deferredCustomLitSettings.Length != 0)
            {
                renderLoop.EnqueuePass(deferredCustomPass);
            }
            
            renderLoop.EnqueuePass(deferredGiPass);
        }
    }
}