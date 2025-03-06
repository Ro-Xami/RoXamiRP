using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRP : RenderPipeline
{
    CameraRender cameraRender = new CameraRender();

    bool GPUInstancing, DynamicBatching;
    public RoXamiRP(bool SRPBatcher , bool GPUInstancing , bool DynamicBatching)
    {
        this.GPUInstancing = GPUInstancing;
        this.DynamicBatching = DynamicBatching;
        GraphicsSettings.useScriptableRenderPipelineBatching = SRPBatcher;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameraRender.Render(context, cameras[i] , GPUInstancing , DynamicBatching);
        }
    }
}