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

    private RenderingData renderingData;
    
    static readonly ScreenSpacePlanarReflectionPass ssprPass = new ScreenSpacePlanarReflectionPass();
    
    public void SetUp(RenderingData renderData)
    {
        renderingData = renderData;
        
        //Opaque==========================================================================
        SetRenderTarget(opaqueCmd);
        opaqueCmd.BeginSample(opaqueBufferName);
        ExecuteBuffer(opaqueCmd);
        
        DrawOpaqueSkybox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings);
        
        opaqueCmd.EndSample(opaqueBufferName);
        ExecuteBuffer(opaqueCmd);
        
        ssprPass.SetUp(renderingData);
        
        //Transparent=======================================================================
        SetRenderTarget(transparentCmd);
        transparentCmd.BeginSample(transparentBufferName);
        ExecuteBuffer(transparentCmd);
        
        DrawTransparent(sortingSettings, drawingSettings, filteringSettings);

        transparentCmd.EndSample(transparentBufferName);
        ExecuteBuffer(transparentCmd);
    }

    private void SetRenderTarget(CommandBuffer cmd)
    {
        cmd.SetRenderTarget(
            renderingData.cameraColorAttachmentId,
            RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
            renderingData.cameraDepthAttachmentId,
            RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
    }

    void DrawOpaqueSkybox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings)
    {
        sortingSettings = new SortingSettings(renderingData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { 
            enableDynamicBatching = renderingData.isDynamicBatching , 
            enableInstancing = renderingData.isGPUInstancing , 
            perObjectData = 
                PerObjectData.ReflectionProbes |
                PerObjectData.LightProbe};
        drawingSettings.SetShaderPassName(1 , toonLitShaderTagId);

        filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        renderingData.context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);

        renderingData.context.DrawSkybox(renderingData.camera);

        CopyCameraColor(opaqueCmd);

        renderingData.context.Submit();
    }

    void CopyCameraColor(CommandBuffer cmd)
    {
        cmd.CopyTexture(renderingData.cameraColorAttachmentId, renderingData.cameraColorCopyTextureID);
        cmd.CopyTexture(renderingData.cameraDepthAttachmentId, renderingData.cameraDepthCopyTextureID);
    }

    void DrawTransparent(SortingSettings sortingSettings, DrawingSettings drawingSettings, FilteringSettings filteringSettings)
    {
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        renderingData.context.DrawRenderers(
            renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        
        CopyCameraColor(transparentCmd);
        
        renderingData.context.Submit();
    }

    private void ExecuteBuffer(CommandBuffer cmd)
    {
        renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        ssprPass.CleanUp();
    }
}