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
            
            deferredDiffusePass = new DeferredDiffusePass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 10);
            
            if (deferredToonLitSettings != null && deferredToonLitSettings.Length != 0)
            {
                deferredCustomPass = new DeferredCustomPass(
                    RenderPassEvent.BeforeRenderingDeferredDiffuse + 11, 
                    deferredToonLitSettings);
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
            
            if (deferredToonLitSettings != null && deferredToonLitSettings.Length != 0)
            {
                renderLoop.EnqueuePass(deferredCustomPass);
            }
            
            renderLoop.EnqueuePass(deferredGiPass);
        }
    }
}