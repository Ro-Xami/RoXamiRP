using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class DeferredToonLitFeature : RoXamiRenderFeature
    {
        [SerializeField]
        DeferredLitSettings[] deferredToonLitSettings;

        GBufferPass gBufferPass;
        ScreenSpaceShadowsPass ssShadowsPass;
        DeferredDiffusePass deferredDiffusePass;
        DeferredCustomPass deferredCustomPass;
        DeferredGiPass deferredGiPass;
        
        public override void Create()
        {
            gBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer + 10);
            
            ssShadowsPass = new ScreenSpaceShadowsPass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 9);
            
            deferredDiffusePass = new DeferredDiffusePass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 10);
            
            if (deferredToonLitSettings != null && deferredToonLitSettings.Length != 0)
            {
                deferredCustomPass = new DeferredCustomPass(
                    RenderPassEvent.BeforeRenderingDeferredDiffuse + 11, 
                    deferredToonLitSettings);
            }

            deferredGiPass = new DeferredGiPass(RenderPassEvent.BeforeRenderingDeferredGI + 10);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
            renderingData.runtimeData.isDeferred = true;
            
            renderer.EnqueuePass(gBufferPass);
            
            renderer.EnqueuePass(ssShadowsPass);
            
            renderer.EnqueuePass(deferredDiffusePass);
            
            if (deferredToonLitSettings != null && deferredToonLitSettings.Length != 0)
            {
                renderer.EnqueuePass(deferredCustomPass);
            }
            
            renderer.EnqueuePass(deferredGiPass);
        }
    }
}