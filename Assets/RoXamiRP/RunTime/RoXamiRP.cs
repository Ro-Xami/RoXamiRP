using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRP : RenderPipeline
{
    readonly CameraRender cameraRender = new CameraRender();

    readonly RoXamiRenderer renderer = default;
    readonly bool GPUInstancing;
    readonly bool DynamicBatching;
    readonly bool isHDR;
    readonly ShadowSettings shadowSettings;
    public RoXamiRP(bool SRPBatcher , bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRenderer renderer, bool isHDR)
    {
        this.GPUInstancing = GPUInstancing;
        this.DynamicBatching = DynamicBatching;
        GraphicsSettings.useScriptableRenderPipelineBatching = SRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.shadowSettings = shadowSettings;
        this.renderer = renderer;
        this.isHDR = isHDR;
    }
    protected override void Render(
        ScriptableRenderContext context, Camera[] cameras
    )
    { }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            cameraRender.Render(
                context, camera, DynamicBatching, GPUInstancing,
                shadowSettings , renderer, isHDR
            );
        }
    }
}