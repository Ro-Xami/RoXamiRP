using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRP : RenderPipeline
{
    readonly CameraRender cameraRender = new CameraRender();
    readonly bool GPUInstancing;
    readonly bool DynamicBatching;
    readonly bool isHDR;
    readonly ShadowSettings shadowSettings;
    readonly RoXamiRendererAsset rendererAsset;
    public RoXamiRP(bool SRPBatcher , bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRendererAsset rendererAsset, bool isHDR)
    {
        this.GPUInstancing = GPUInstancing;
        this.DynamicBatching = DynamicBatching;
        GraphicsSettings.useScriptableRenderPipelineBatching = SRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.shadowSettings = shadowSettings;
        this.isHDR = isHDR;
        this.rendererAsset = rendererAsset;
    }
    protected override void Render(
        ScriptableRenderContext context, Camera[] cameras
    )
    { }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            var cameraAdditionalData = camera.GetRoXamiAdditionalCameraData();
            cameraRender.Render(context, camera, shadowSettings , rendererAsset);
        }
    }
}