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
    
    static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("ToonUnlit");
    static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");
    
    static readonly int cameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");
    static readonly int cameraOpaqueColorTextureID = Shader.PropertyToID("_CameraOpaqueColorTexture");
    
    public void Render()
    {
        GetCameraColorDepthTexture();
        ClearCmdRenderTarget();
        
        cmd.BeginSample(bufferName);
        ExecuteBuffer();
        
        DrawOpaqueSkyox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings);
        CopyCameraDepth();
        DrawTransparent(sortingSettings, drawingSettings, filteringSettings);

        cmd.EndSample(bufferName);
        ExecuteBuffer();
        CameraRender.renderingData.context.Submit();
    }

    void GetCameraColorDepthTexture()
    {
        cmd.GetTemporaryRT(
            CameraRender.renderingData.cameraColorBufferId, 
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            0, FilterMode.Bilinear,
            CameraRender.renderingData.isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);

        cmd.GetTemporaryRT(
            CameraRender.renderingData.cameraDepthBufferId, 
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            32, FilterMode.Point, RenderTextureFormat.Depth);
        
        cmd.SetRenderTarget(
            CameraRender.renderingData.cameraColorBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
            CameraRender.renderingData.cameraDepthBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
    }

    void DrawOpaqueSkyox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings)
    {
        sortingSettings = new SortingSettings(CameraRender.renderingData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { 
            enableDynamicBatching = CameraRender.renderingData.isDynamicBatching , 
            enableInstancing = CameraRender.renderingData.isGPUInstancing , 
            perObjectData = 
                PerObjectData.ReflectionProbes |
                PerObjectData.LightProbe};
        drawingSettings.SetShaderPassName(1 , toonLitShaderTagId);

        filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        CameraRender.renderingData.context.DrawRenderers(CameraRender.renderingData.cullingResults, ref drawingSettings, ref filteringSettings);

        CameraRender.renderingData.context.DrawSkybox(CameraRender.renderingData.camera);
    }

    void DrawTransparent(SortingSettings sortingSettings, DrawingSettings drawingSettings, FilteringSettings filteringSettings)
    {
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        CameraRender.renderingData.context.DrawRenderers(CameraRender.renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
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

    void CopyCameraDepth()
    {
        cmd.GetTemporaryRT(
            cameraDepthTextureID, 
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            32, FilterMode.Point, RenderTextureFormat.Depth);

        cmd.GetTemporaryRT(
            cameraOpaqueColorTextureID,
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            0, FilterMode.Bilinear,
            CameraRender.renderingData.isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
        
        cmd.CopyTexture(CameraRender.renderingData.cameraDepthBufferId, cameraDepthTextureID);
        cmd.CopyTexture(CameraRender.renderingData.cameraColorBufferId, cameraOpaqueColorTextureID);
        ExecuteBuffer();
    }

    private void ExecuteBuffer()
    {
        CameraRender.renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        cmd.ReleaseTemporaryRT(cameraDepthTextureID);
        cmd.ReleaseTemporaryRT(cameraOpaqueColorTextureID);
        cmd.ReleaseTemporaryRT(CameraRender.renderingData.cameraColorBufferId);
        cmd.ReleaseTemporaryRT(CameraRender.renderingData.cameraDepthBufferId);
    }
}