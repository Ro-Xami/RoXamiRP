using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRP : RenderPipeline
{
    readonly CameraRender cameraRender = new CameraRender();
    private const bool enableSrpBatcher = true;
    private const bool lightsUseLinearIntensity = true;
    readonly ShadowSettings shadowSettings;
    readonly ShaderAsset shaderAsset;
    readonly RoXamiRendererAsset[] rendererAssets;

    public RoXamiRP(ShadowSettings shadowSettings, ShaderAsset shaderAsset, RoXamiRendererAsset[] rendererAssets)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = enableSrpBatcher;
        GraphicsSettings.lightsUseLinearIntensity = lightsUseLinearIntensity;
        this.shadowSettings = shadowSettings;
        this.shaderAsset = shaderAsset;
        this.rendererAssets = rendererAssets;
    }
    protected override void Render(
        ScriptableRenderContext context, Camera[] cameras
    )
    { }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            var additionalCameraData = camera.GetRoXamiAdditionalCameraData();
            
            RoXamiRendererAsset renderAsset = 
                additionalCameraData.roXamiRendererAssetID < rendererAssets.Length ?
                    rendererAssets[additionalCameraData.roXamiRendererAssetID] : 
                    RoXamiRendererAsset.defaultAsset;
 
            cameraRender.Render(context, camera, additionalCameraData, shadowSettings , renderAsset, shaderAsset);
        }
    }
}