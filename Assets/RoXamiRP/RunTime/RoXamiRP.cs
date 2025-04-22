using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRP : RenderPipeline
{
    CameraRender cameraRender = new CameraRender();

    bool GPUInstancing, DynamicBatching;
    ShadowSettings shadowSettings;
    public RoXamiRP(bool SRPBatcher , bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings)
    {
        this.GPUInstancing = GPUInstancing;
        this.DynamicBatching = DynamicBatching;
        GraphicsSettings.useScriptableRenderPipelineBatching = SRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.shadowSettings = shadowSettings;
    }
    protected override void Render(
        ScriptableRenderContext context, Camera[] cameras
    )
    { }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameraRender.Render(
                context, cameras[i], DynamicBatching, GPUInstancing,
                shadowSettings
            );
        }
    }
}