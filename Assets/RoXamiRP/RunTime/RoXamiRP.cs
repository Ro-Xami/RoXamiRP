using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRP : RenderPipeline
{
    CameraRender cameraRender = new CameraRender();

    RoXamiRenderer renderer = default;
    bool GPUInstancing, DynamicBatching;
    ShadowSettings shadowSettings;
    public RoXamiRP(bool SRPBatcher , bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRenderer renderer)
    {
        this.GPUInstancing = GPUInstancing;
        this.DynamicBatching = DynamicBatching;
        GraphicsSettings.useScriptableRenderPipelineBatching = SRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.shadowSettings = shadowSettings;
        this.renderer = renderer;
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
                shadowSettings //, renderer
            );
        }
    }
}