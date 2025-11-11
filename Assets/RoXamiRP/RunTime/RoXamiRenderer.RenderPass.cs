using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    partial class RoXamiRenderer
    {
        private readonly List<RoXamiRenderPass> activePasses = 
            new List<RoXamiRenderPass>(32);

        private readonly LightingPass lightPass = 
            new LightingPass(RenderPassEvent.BeforeRenderingShadows + 10);
        private readonly ShadowsCasterPass shadowsCasterPass = 
            new ShadowsCasterPass(RenderPassEvent.BeforeRenderingShadows + 11);

        private readonly RenderingPrePasses prePasses = 
            new RenderingPrePasses(RenderPassEvent.BeforeRenderingPrePasses + 10);

        // private readonly GBufferPass gBufferPass = 
        //     new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer + 10);
        //
        // private readonly ScreenSpaceShadowsPass ssShadowsPass =
        //     new ScreenSpaceShadowsPass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 9);
        //
        // private readonly DeferredDiffusePass _deferredDiffusePass = 
        //     new DeferredDiffusePass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 10);

        private readonly ForwardOpaquePass forwardOpaquePass =
            new ForwardOpaquePass(RenderPassEvent.BeforeRenderingOpaques + 10);

        private readonly DrawSkyboxPass drawSkyboxPass = 
             new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox + 10);

        private readonly CopyCameraColorPass copyCameraColorPass =
            new CopyCameraColorPass(RenderPassEvent.AfterRenderingSkybox);
        
        private readonly CopyCameraDepthPass copyCameraDepthPass =
            new CopyCameraDepthPass(RenderPassEvent.AfterRenderingSkybox);

        private readonly ForwardTransparentPass forwardTransparentPass =
            new ForwardTransparentPass(RenderPassEvent.BeforeRenderingTransparents + 10);

        private readonly PostPass postPass = 
            new PostPass(RenderPassEvent.BeforeRenderingPostProcessing + 10);

        private readonly AntialiasingPass antialiasingPass = 
            new AntialiasingPass(RenderPassEvent.AfterRendering);

        private readonly FinalBlitPass finalBlitPass = 
            new FinalBlitPass(RenderPassEvent.AfterRendering + 1);

#if UNITY_EDITOR
        private readonly RenderingDebugPass renderingDebugPass =
            new RenderingDebugPass(RenderPassEvent.AfterRendering + 100);
#endif
        
        internal void InitializedActiveRenderPass(List<RoXamiRenderFeature> features)
        {
            activePasses.Clear();
            AddBaseRenderPasses(renderingData);
            AddRenderFeatures(features);
            SortStable(activePasses);
        }
        
        private void CameraSetup(CommandBuffer rendererCommandBuffer, ref RenderingData renderingData)
        {
            foreach (var pass in activePasses)
            {
                pass.SetUp(rendererCommandBuffer, ref renderingData);
            }
        }
        
        private void ExecuteRoXamiRenderPass(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            foreach (var pass in activePasses)
            {
                //Debug.Log(renderingData.rendererAsset.name + ":" + pass.GetType().Name);
                pass.Execute(context, ref renderingData);
            }
        }
        
        private void CameraCleanUp(CommandBuffer cmd)
        {
            foreach (var pass in activePasses)
            {
                pass.CleanUp(cmd);
            }
        }
        
        private void AddBaseRenderPasses(RenderingData renderingData)
        {
            if (renderingData.rendererSettings.enableLighting)
            {
                activePasses.Add(lightPass);
                activePasses.Add(shadowsCasterPass);
            }
            activePasses.Add(prePasses);
            
            //=========================================================================
            //Deferred
            // if (renderingData.rendererAsset.rendererSettings.enableDeferredRendering && 
            //     renderingData.rendererAsset.rendererSettings.enableLighting)
            // {
            //     activePasses.Add(gBufferPass);
            //     activePasses.Add(deferredPass);
            //     activePasses.Add(ssShadowsPass);
            // }

            //=========================================================================
            //Forward
            activePasses.Add(forwardOpaquePass);
            
            if (renderingData.cameraData.additionalCameraData.backgroundType == BackgroundType.Skybox)
            {
                 activePasses.Add(drawSkyboxPass);
            }

            if (renderingData.rendererSettings.copyColorAfterSkybox)
            {
                activePasses.Add(copyCameraColorPass);
            }

            if (renderingData.rendererSettings.copyDepthAfterOpaque)
            {
                activePasses.Add(copyCameraDepthPass);
            }
            
            activePasses.Add(forwardTransparentPass);

            //=========================================================================
            //Post Antialiasing FinalBlit
            if (renderingData.runtimeData is { isPost: false, isAntialiasing: false, isCameraStackFinally: true })
            {
                activePasses.Add(finalBlitPass);
            }
            else
            {
                if (renderingData.runtimeData.isPost)
                {
                    activePasses.Add(postPass);
                }

                if (renderingData.runtimeData.isAntialiasing)
                {
                    activePasses.Add(antialiasingPass);
                }
            }
            
            #if UNITY_EDITOR
            
            if (RoXamiFeatureManager.Instance.IsActive(RoXamiFeatureStack.RenderingDebug) && 
                renderingData.cameraData.additionalCameraData.cameraRenderType == CameraRenderType.Base)
            {
                activePasses.Add(renderingDebugPass);
            }
            
            #endif
        }

        private void AddRenderFeatures(List<RoXamiRenderFeature> features)
        {
            foreach (var feature in features)
            {
                if (!feature || !feature.isActive)
                {
                    continue;
                }
                feature.AddRenderPasses(this, ref renderingData);
            }
        }
        
        private void SortStable(List<RoXamiRenderPass> list)
        {
            for (int i = 1; i < activePasses.Count; ++i)
            {
                RoXamiRenderPass curr = list[i];

                var j = i - 1;
                for (; j >= 0 && curr < list[j]; --j)
                    list[j + 1] = list[j];

                list[j + 1] = curr;
            }
        }
        
        public void EnqueuePass(RoXamiRenderPass pass)
        {
            activePasses.Add(pass);
        }
    }
}