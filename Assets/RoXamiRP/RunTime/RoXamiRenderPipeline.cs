using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class RoXamiRenderPipline : RenderPipeline
    {
        private const bool enableSrpBatcher = true;
        private const bool lightsUseLinearIntensity = true;
        private RoXamiRPAsset rpAsset;

        public RoXamiRenderPipline(RoXamiRPAsset rpAsset)
        {
            this.rpAsset = rpAsset;
            GraphicsSettings.useScriptableRenderPipelineBatching = enableSrpBatcher;
            GraphicsSettings.lightsUseLinearIntensity = lightsUseLinearIntensity;
            
            RTHandles.Initialize(Screen.width, Screen.height);
            RoXamiRTHandlePool.ReleasePool();
            renderers.Clear();
        }

        readonly Dictionary<int,RoXamiRenderer> renderers = new Dictionary<int, RoXamiRenderer>();

        protected override void Render(
            ScriptableRenderContext context, Camera[] cameras
        )
        {
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            if (cameras is {Count: 0})
            {
                return;
            }

#if UNITY_EDITOR
            RoXamiVolume.Instance.Update();
#endif
            
            foreach (var baseCamera in cameras)
            {
                //Base Camera
                var additionalCameraData = baseCamera.GetRoXamiAdditionalCameraData();
                if (additionalCameraData.cameraRenderType == CameraRenderType.Overlay)
                {
                    continue;
                }
                
                if (!baseCamera.TryGetCullingParameters(out ScriptableCullingParameters p))
                {
                    continue;
                }

                RoXamiRenderer cameraStackRenderer;
                if (renderers.TryGetValue(baseCamera.GetInstanceID(), out var renderer))
                {
                    if (renderer == null)
                    {
                        renderer = new RoXamiRenderer();
                    }
                    cameraStackRenderer = renderer;
                }
                else
                {
                    renderers[baseCamera.GetInstanceID()] = new RoXamiRenderer();
                    cameraStackRenderer = renderers[baseCamera.GetInstanceID()];
                }
                
                var rendererAsset = 
                    additionalCameraData.roXamiRendererAssetID < rpAsset.rendererAssets.Length && additionalCameraData.roXamiRendererAssetID > 0?
                        rpAsset.rendererAssets[additionalCameraData.roXamiRendererAssetID]: RoXamiRendererAsset.defaultAsset;

                cameraStackRenderer.InitializedRenderingData(
                    context, baseCamera, additionalCameraData, 
                    rpAsset, p, true);

                cameraStackRenderer.InitializedActiveRenderPass(rendererAsset.roXamiRenderFeatures);
                cameraStackRenderer.Render(context);
            }
            
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var renderer in renderers)
            {
                if (renderer.Value != null)
                {
                    renderer.Value.Dispose();
                }
            }
        }
    }
}