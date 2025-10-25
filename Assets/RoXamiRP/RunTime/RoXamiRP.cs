using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class RoXamiRP : RenderPipeline
    {
        readonly CameraRender cameraRender = new CameraRender();
        private const bool enableSrpBatcher = true;
        private const bool lightsUseLinearIntensity = true;
        private readonly ShadowSettings shadowSettings;
        private readonly ShaderAsset shaderAsset;
        private readonly RoXamiRendererAsset[] rendererAssets;
        private readonly CommonSettings commonSettings;
        private readonly AntialiasingSettings antialiasingSettings;

        public RoXamiRP(ShadowSettings shadowSettings, ShaderAsset shaderAsset, RoXamiRendererAsset[] rendererAssets, 
            CommonSettings commonSettings, AntialiasingSettings antialiasingSettings)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = enableSrpBatcher;
            GraphicsSettings.lightsUseLinearIntensity = lightsUseLinearIntensity;
            this.shadowSettings = shadowSettings;
            this.shaderAsset = shaderAsset;
            this.rendererAssets = rendererAssets;
            this.commonSettings = commonSettings;
            this.antialiasingSettings = antialiasingSettings;
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
            
            foreach (var camera in cameras)
            {
                //Base Camera
                var additionalCameraData = camera.GetRoXamiAdditionalCameraData();
                if (additionalCameraData.cameraRenderType == CameraRenderType.Overlay)
                {
                    continue;
                }
                
                RenderBaseCamera(context, additionalCameraData, camera, out var isSingleBaseCamera);

                //Overlay Camera
                if (!isSingleBaseCamera)
                {
                    for (int i = 0; i < additionalCameraData.cameraStack.Count; i++)
                    {
                        RenderOverlayCamera(context, additionalCameraData, i, camera);
                    }
                }
            }
        }
        
        private void RenderBaseCamera(ScriptableRenderContext context, AdditionalCameraData additionalCameraData, Camera camera,
            out bool isSingleBaseCamera)
        {
            var renderAsset =
                additionalCameraData.roXamiRendererAssetID < rendererAssets.Length ? 
                    rendererAssets[additionalCameraData.roXamiRendererAssetID]
                    : RoXamiRendererAsset.defaultAsset;

            isSingleBaseCamera = additionalCameraData.cameraStack == null || 
                                 additionalCameraData.cameraStack.Count == 0;
            cameraRender.Render(
                context, camera, additionalCameraData, 
                commonSettings, shadowSettings, renderAsset, 
                shaderAsset, antialiasingSettings,
                isSingleBaseCamera);
        }

        private void RenderOverlayCamera(ScriptableRenderContext context, AdditionalCameraData additionalCameraData, int i,
            Camera camera)
        {
            var cameraStack = additionalCameraData.cameraStack[i];
            if (cameraStack == null)
            {
                additionalCameraData.cameraStack.Remove(cameraStack);
                Debug.LogError("camera stack" + i + " is null!");
                return;
            }
                        
            var cameraStackAdditionalData = cameraStack.GetRoXamiAdditionalCameraData();
            if (cameraStackAdditionalData.cameraRenderType != CameraRenderType.Overlay)
            {
                #if UNITY_EDITOR
                Debug.LogError(camera.name + ": camera stack" + i + " is not Overlay!");
                #endif
                
                return;
            }
                    
            var cameraStackRenderAsset = 
                cameraStackAdditionalData.roXamiRendererAssetID < rendererAssets.Length ? 
                    rendererAssets[cameraStackAdditionalData.roXamiRendererAssetID]
                    : RoXamiRendererAsset.defaultAsset;

            bool isFinalOverlayCamera = i == additionalCameraData.cameraStack.Count - 1;
            bool isAB = i % 2 != 0;
                        
            cameraRender.Render(
                context, cameraStack, cameraStackAdditionalData, 
                commonSettings, shadowSettings, cameraStackRenderAsset, 
                shaderAsset, antialiasingSettings,
                isFinalOverlayCamera);
        }
    }
}