using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRP : RenderPipeline
{
    readonly CameraRender cameraRender = new CameraRender();

    readonly RoXamiRendererAsset _rendererAsset = default;
    readonly bool GPUInstancing;
    readonly bool DynamicBatching;
    readonly bool isHDR;
    readonly ShadowSettings shadowSettings;
    public RoXamiRP(bool SRPBatcher , bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRendererAsset rendererAsset, bool isHDR)
    {
        this.GPUInstancing = GPUInstancing;
        this.DynamicBatching = DynamicBatching;
        GraphicsSettings.useScriptableRenderPipelineBatching = SRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.shadowSettings = shadowSettings;
        this._rendererAsset = rendererAsset;
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
                context, camera, GPUInstancing, DynamicBatching,
                shadowSettings , _rendererAsset, isHDR
            );
        }
    }
}