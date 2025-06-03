using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class ForwardPass
{
    static readonly string bufferName = "RoXami Forward";
    static readonly CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };
    
    static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");
    
    public void Render()
    {
        cmd.GetTemporaryRT(
            CameraRender.renderingData.frameBufferId, CameraRender. renderingData.width, CameraRender.renderingData.height,
            32, FilterMode.Bilinear,
            CameraRender.renderingData.isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default
        );
        cmd.SetRenderTarget(
            CameraRender.renderingData.frameBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );

        ClearCmdRenderTarget();
        
        cmd.BeginSample(bufferName);
        ExecuteBuffer();
        
        DrawForward();

        cmd.EndSample(bufferName);
        ExecuteBuffer();
        CameraRender.renderingData.context.Submit();
    }

    private void DrawForward()
    {
        SortingSettings sortingSettings = new SortingSettings(CameraRender.renderingData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { 
            enableDynamicBatching = CameraRender.renderingData.isDynamicBatching , 
            enableInstancing = CameraRender.renderingData.isGPUInstancing , 
            perObjectData = 
                PerObjectData.ReflectionProbes |
                PerObjectData.LightProbe};
        drawingSettings.SetShaderPassName(1 , toonLitShaderTagId);

        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        CameraRender.renderingData.context.DrawRenderers(CameraRender.renderingData.cullingResults, ref drawingSettings, ref filteringSettings);

        CameraRender.renderingData.context.DrawSkybox(CameraRender.renderingData.camera);
        
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        CameraRender.renderingData.context.DrawRenderers(CameraRender.renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
    }

    private void ExecuteBuffer()
    {
        CameraRender.renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        cmd.ReleaseTemporaryRT(CameraRender.renderingData.frameBufferId);
    }
    
    void ClearCmdRenderTarget()
    {
        CameraClearFlags flags = CameraRender.renderingData.camera.clearFlags;
        cmd.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                CameraRender.renderingData.camera.backgroundColor.linear : Color.clear
        );
    }
}