using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
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
        ScreenSpaceShadowsPass ssShadowsPass;
        DeferredDiffusePass deferredDiffusePass;
        DeferredCustomPass deferredCustomPass;
        DeferredGiPass deferredGiPass;
        
        public override void Create()
        {
            gBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer + 5);
            
            ssShadowsPass = new ScreenSpaceShadowsPass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 4);
            
            deferredDiffusePass = new DeferredDiffusePass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 5, deferredToonLitMaterial);
            
            if (deferredCustomLitSettings != null && deferredCustomLitSettings.Length != 0)
            {
                deferredCustomPass = new DeferredCustomPass(
                    RenderPassEvent.BeforeRenderingDeferredDiffuse + 11, 
                    deferredCustomLitSettings);
            }

            deferredGiPass = new DeferredGiPass(RenderPassEvent.BeforeRenderingDeferredGI + 5);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!IsGameOrSceneCamera(renderingData.cameraData.camera) &&
                renderingData.cameraData.camera.cameraType != CameraType.Preview) return;
#endif

            renderer.EnqueuePass(gBufferPass);
            
            renderer.EnqueuePass(ssShadowsPass);
            
            renderer.EnqueuePass(deferredDiffusePass);
            
            if (deferredCustomLitSettings != null && deferredCustomLitSettings.Length != 0)
            {
                renderer.EnqueuePass(deferredCustomPass);
            }
            
            renderer.EnqueuePass(deferredGiPass);
        }
    }
}