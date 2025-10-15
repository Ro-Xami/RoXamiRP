using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class RoXamiRenderer
    {
        readonly List<RoXamiRenderPass> activePasses = 
            new List<RoXamiRenderPass>(32);

        private readonly LightingPass lightPass = 
            new LightingPass(RenderPassEvent.BeforeRenderingShadows + 10);

        private readonly RenderingPrePasses prePasses = 
            new RenderingPrePasses(RenderPassEvent.BeforeRenderingPrePasses + 10);

        private readonly GBufferPass gBufferPass = 
            new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer + 10);

        private readonly ScreenSpaceShadowsPass ssShadowsPass =
            new ScreenSpaceShadowsPass(RenderPassEvent.BeforeRenderingDeferredLights + 9);

        private readonly DeferredPass deferredPass = 
            new DeferredPass(RenderPassEvent.BeforeRenderingDeferredLights + 10);

        private readonly ForwardOpaquePass forwardOpaquePass =
            new ForwardOpaquePass(RenderPassEvent.BeforeRenderingOpaques + 10);

        private readonly DrawSkyboxPass drawSkyboxPass = 
             new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox + 10);

        private readonly ForwardTransparentPass forwardTransparentPass =
            new ForwardTransparentPass(RenderPassEvent.BeforeRenderingTransparents + 10);

        private readonly PostPass postPass = 
            new PostPass(RenderPassEvent.BeforeRenderingPostProcessing + 10);

        private readonly AntialiasingPass antialiasingPass = 
            new AntialiasingPass(RenderPassEvent.AfterRendering);

        private readonly FinalBlitPass finalBlitPass = 
            new FinalBlitPass(RenderPassEvent.AfterRendering + 1);

        public RoXamiRenderer()
        {
        }

        public void InitializedActiveRenderPass(RoXamiRendererAsset asset, ref RenderingData renderingData)
        {
            activePasses.Clear();
            AddBaseRenderPasses(renderingData);
            AddRenderFeatures(asset, ref renderingData);
            SortStable(activePasses);
        }

        public void CameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            foreach (var pass in activePasses)
            {
                pass.SetUp(cmd, ref renderingData);
            }
        }

        public void ExecuteRoXamiRenderPass(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            foreach (var pass in activePasses)
            {
                pass.Execute(context, ref renderingData);
            }
        }

        public void CameraCleanUp()
        {
            foreach (var pass in activePasses)
            {
                pass.CleanUp();
            }
        }

        private void AddBaseRenderPasses(RenderingData renderingData)
        {
            if (renderingData.rendererAsset.rendererSettings.enableLighting)
            {
                activePasses.Add(lightPass);
            }
            activePasses.Add(prePasses);
            
            //=========================================================================
            //Deferred
            if (renderingData.rendererAsset.rendererSettings.enableDeferredRendering && 
                renderingData.rendererAsset.rendererSettings.enableLighting)
            {
                activePasses.Add(gBufferPass);
                activePasses.Add(deferredPass);
                activePasses.Add(ssShadowsPass);
            }

            //=========================================================================
            //Forward
            activePasses.Add(forwardOpaquePass);
            
            if (renderingData.cameraData.additionalCameraData.backgroundType == BackgroundType.Skybox)
            {
                 activePasses.Add(drawSkyboxPass);
            }

            activePasses.Add(forwardTransparentPass);

            //=========================================================================
            //Post Antialiasing FinalBlit
            if (renderingData.runtimeData is { isPost: false, isAntialiasing: false, isFinalBlit: true })
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
            
        }

        private void AddRenderFeatures(RoXamiRendererAsset asset, ref RenderingData renderingData)
        {
            foreach (var feature in asset.roXamiRenderFeatures)
            {
                if (feature == null || !feature.isActive)
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