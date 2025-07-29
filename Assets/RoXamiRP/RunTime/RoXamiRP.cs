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

    public RoXamiRP(ShadowSettings shadowSettings, ShaderAsset shaderAsset)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = enableSrpBatcher;
        GraphicsSettings.lightsUseLinearIntensity = lightsUseLinearIntensity;
        this.shadowSettings = shadowSettings;
        this.shaderAsset = shaderAsset;
    }
    protected override void Render(
        ScriptableRenderContext context, Camera[] cameras
    )
    { }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            var asset = camera.GetRoXamiRendererAsset();
            cameraRender.Render(context, camera, shadowSettings , asset, shaderAsset);
        }
    }
}