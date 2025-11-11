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
        private readonly RoXamiRPAsset rpAsset;
        
        HashSet<RoXamiRenderer> m_Renderers = new HashSet<RoXamiRenderer>();

        public RoXamiRenderPipline(RoXamiRPAsset rpAsset)
        {
            this.rpAsset = rpAsset;
            GraphicsSettings.useScriptableRenderPipelineBatching = enableSrpBatcher;
            GraphicsSettings.lightsUseLinearIntensity = lightsUseLinearIntensity;
            
            RTHandles.Initialize(Screen.width, Screen.height);
            RoXamiRTHandlePool.ReleasePool();
            
            m_Renderers.Clear();
        }

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
            
            if (RoXamiRTHandlePool.GetRTHandlesCount() > 32)
            {
                RoXamiRTHandlePool.DebugRTHandles();
                RoXamiRTHandlePool.ReleasePool();
            }
            
            foreach (var baseCamera in cameras)
            {
                //Base Camera
                var baseAdditionalCameraData = baseCamera.GetRoXamiAdditionalCameraData();
                if (baseAdditionalCameraData.cameraRenderType != CameraRenderType.Base) 
                    continue;

                RoXamiRenderer baseRenderer = baseAdditionalCameraData.renderer;
                m_Renderers.Add(baseRenderer);
                
                bool isBaseFinally =
                    baseAdditionalCameraData.cameraStack != null && baseAdditionalCameraData.cameraStack.Count > 0;

                if (TryGetCull(baseCamera))
                {
                    RenderSingleCamera(context, baseAdditionalCameraData, baseRenderer, baseCamera, isBaseFinally);
                }

                if (!isBaseFinally) continue;
                
                for (int i = 0; i < baseAdditionalCameraData.cameraStack.Count; i++)
                {
                    var cameraStack = baseAdditionalCameraData.cameraStack[i];
                    bool isOverlayFinally = i == baseAdditionalCameraData.cameraStack.Count - 1;
                    
                    if (!cameraStack) continue;
                    var overlayAdditionalCameraData = cameraStack.GetRoXamiAdditionalCameraData();
                    
                    if (overlayAdditionalCameraData.cameraRenderType != CameraRenderType.Overlay)
                        continue;

                    var overlayRenderer = overlayAdditionalCameraData.renderer;
                    m_Renderers.Add(overlayRenderer);
                    
                    if (TryGetCull(cameraStack))
                    {
                        RenderSingleCamera(context, overlayAdditionalCameraData, overlayRenderer, cameraStack, isOverlayFinally);
                    }
                }
            }
            
        }

        private bool TryGetCull(Camera baseCamera)
        {
            if (baseCamera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                return true;
            }

            return false;
        }

        private void RenderSingleCamera(ScriptableRenderContext context, AdditionalCameraData additionalCameraData,
            RoXamiRenderer cameraStackRenderer, Camera baseCamera, bool isFinaleCamera)
        {
            var rendererAsset = additionalCameraData.roXamiRendererAsset ?
                additionalCameraData.roXamiRendererAsset: RoXamiRendererAsset.defaultAsset;

            cameraStackRenderer.InitializedRenderingData(
                context, baseCamera, additionalCameraData, 
                rpAsset, rendererAsset, true, out bool cull);

            if (!cull) return;

            cameraStackRenderer.InitializedActiveRenderPass(rendererAsset.roXamiRenderFeatures);
            cameraStackRenderer.Render(context);
        }


        protected override void Dispose(bool disposing)
        {
            RoXamiRTHandlePool.ReleasePool();
            
            foreach (var renderer in m_Renderers)
            {
                if (renderer != null)
                {
                    renderer.Dispose();
                }
            }
        }
    }
}