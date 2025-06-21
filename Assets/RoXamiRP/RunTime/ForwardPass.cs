using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class ForwardPass
{
    static readonly string opaqueBufferName = "RoXami Forward Opaque";
    static readonly CommandBuffer opaqueCmd = new CommandBuffer()
    {
        name = opaqueBufferName
    };
    
    static readonly string transparentBufferName = "RoXami Forward Transparent";
    static readonly CommandBuffer transparentCmd = new CommandBuffer()
    {
        name = transparentBufferName
    };

    static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("ToonUnlit");
    static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");
    
    static readonly int cameraOpaqueDepthTextureID = Shader.PropertyToID("_CameraOpaqueDepthTexture");
    static readonly int cameraOpaqueColorTextureID = Shader.PropertyToID("_CameraOpaqueColorTexture");
    
    
    static readonly int worldSpacePositionTextureID = Shader.PropertyToID("_WorldSpacePositionTexture");
    
    public void Render()
    {
        SetUp();
        
        //Opaque==========================================================================
        opaqueCmd.BeginSample(opaqueBufferName);
        ExecuteBuffer(opaqueCmd);
        
        DrawOpaqueSkybox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings);
        
        opaqueCmd.EndSample(opaqueBufferName);
        ExecuteBuffer(opaqueCmd);
        
        //Transparent=======================================================================
        transparentCmd.BeginSample(transparentBufferName);
        ExecuteBuffer(transparentCmd);
        
        DrawTransparent(sortingSettings, drawingSettings, filteringSettings);

        transparentCmd.EndSample(transparentBufferName);
        ExecuteBuffer(transparentCmd);
    }

    void SetUp()
    {
        opaqueCmd.GetTemporaryRT(
            CameraRender.renderingData.cameraColorBufferId, 
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            0, FilterMode.Bilinear,
            CameraRender.renderingData.isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);

        opaqueCmd.GetTemporaryRT(
            CameraRender.renderingData.cameraDepthBufferId, 
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            32, FilterMode.Point, RenderTextureFormat.Depth);

        opaqueCmd.GetTemporaryRT(
            cameraOpaqueDepthTextureID, 
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            32, FilterMode.Point, RenderTextureFormat.Depth);

        opaqueCmd.GetTemporaryRT(
            cameraOpaqueColorTextureID,
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            0, FilterMode.Bilinear,
            CameraRender.renderingData.isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
        
        opaqueCmd.SetRenderTarget(
            CameraRender.renderingData.cameraColorBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
            CameraRender.renderingData.cameraDepthBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        
        CameraClearFlags flags = CameraRender.renderingData.camera.clearFlags;
        opaqueCmd.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                CameraRender.renderingData.camera.backgroundColor.linear : Color.clear
        );
    }

    void DrawOpaqueSkybox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings)
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
        
        CopyOpaqueDepthColor();
        //CalculatePositionWS();

        CameraRender.renderingData.context.Submit();
    }

    void CopyOpaqueDepthColor()
    {
        opaqueCmd.CopyTexture(CameraRender.renderingData.cameraDepthBufferId, cameraOpaqueDepthTextureID);
        opaqueCmd.CopyTexture(CameraRender.renderingData.cameraColorBufferId, cameraOpaqueColorTextureID);
    }

    void DrawTransparent(SortingSettings sortingSettings, DrawingSettings drawingSettings, FilteringSettings filteringSettings)
    {
        transparentCmd.SetRenderTarget(
            CameraRender.renderingData.cameraColorBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
            CameraRender.renderingData.cameraDepthBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        CameraRender.renderingData.context.DrawRenderers(CameraRender.renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        
        CameraRender.renderingData.context.Submit();
    }

    void CalculatePositionWS()
    {
        opaqueCmd.GetTemporaryRT(
            worldSpacePositionTextureID,
            CameraRender.renderingData.width, CameraRender.renderingData.height,
            0, FilterMode.Bilinear, RenderTextureFormat.Default,RenderTextureReadWrite.Linear,
            1,true);
        
        ComputeShader cs = CameraRender.renderingData.renderer.depthToPositionWSCompute;
        int kernel = cs.FindKernel("CSMain");

        opaqueCmd.SetComputeTextureParam(cs, kernel,"Result",worldSpacePositionTextureID);
        
        int threadGroupX = Mathf.CeilToInt(CameraRender.renderingData.width / 8.0f);
        int threadGroupY = Mathf.CeilToInt(CameraRender.renderingData.height / 8.0f);
        opaqueCmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
        
    }

    private void ExecuteBuffer(CommandBuffer cmd)
    {
        CameraRender.renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        opaqueCmd.ReleaseTemporaryRT(cameraOpaqueDepthTextureID);
        opaqueCmd.ReleaseTemporaryRT(cameraOpaqueColorTextureID);
        opaqueCmd.ReleaseTemporaryRT(worldSpacePositionTextureID);
        opaqueCmd.ReleaseTemporaryRT(CameraRender.renderingData.cameraColorBufferId);
        opaqueCmd.ReleaseTemporaryRT(CameraRender.renderingData.cameraDepthBufferId);
    }
}