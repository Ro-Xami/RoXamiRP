using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class ForwardPass : RoXamiRenderPass
{
    public ForwardPass(RenderPassEvent evt)
    {
        renderPassEvent = evt;
    }
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

    private RenderingData renderingData;
    private ScriptableRenderContext context;
    
    //static readonly ScreenSpacePlanarReflectionPass ssprPass = new ScreenSpacePlanarReflectionPass();

    public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderData)
    {
        renderingData = renderData;
        context = scriptableRenderContext;
        
        //Opaque==========================================================================
        SetRenderTarget(opaqueCmd);
        opaqueCmd.BeginSample(opaqueBufferName);
        ExecuteCommandBuffer(context, opaqueCmd);
        
        DrawOpaqueSkybox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings);
        
        opaqueCmd.EndSample(opaqueBufferName);
        ExecuteCommandBuffer(context, opaqueCmd);
        
        //ssprPass.SetUp(context, renderingData);
        
        //Transparent=======================================================================
        SetRenderTarget(transparentCmd);
        transparentCmd.BeginSample(transparentBufferName);
        ExecuteCommandBuffer(context, transparentCmd);
        
        DrawTransparent(sortingSettings, drawingSettings, filteringSettings);

        transparentCmd.EndSample(transparentBufferName);
        ExecuteCommandBuffer(context, transparentCmd);
    }
    
    public override void CleanUp()
    {
        //ssprPass.CleanUp();
    }

    private void SetRenderTarget(CommandBuffer cmd)
    {
        cmd.SetRenderTarget(
            ShaderDataID.cameraColorAttachmentId,
            RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
            ShaderDataID.cameraDepthAttachmentId,
            RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
    }

    void DrawOpaqueSkybox(out SortingSettings sortingSettings, out DrawingSettings drawingSettings, out FilteringSettings filteringSettings)
    {
        sortingSettings = new SortingSettings(renderingData.cameraData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        drawingSettings = new DrawingSettings(ShaderDataID.unlitShaderTagId, sortingSettings) { 
            enableDynamicBatching = renderingData.rendererAsset.commonSettings.enableDynamicBatching , 
            enableInstancing = renderingData.rendererAsset.commonSettings.enableGpuInstancing , 
            perObjectData = 
                PerObjectData.ReflectionProbes |
                PerObjectData.LightProbe};
        drawingSettings.SetShaderPassName(1 , ShaderDataID.toonLitShaderTagId);

        filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(renderingData.cameraData.camera);

        CopyCameraColor(opaqueCmd);

        context.Submit();
    }

    void CopyCameraColor(CommandBuffer cmd)
    {
        cmd.CopyTexture(ShaderDataID.cameraColorAttachmentId, ShaderDataID.cameraColorCopyTextureID);
        cmd.CopyTexture(ShaderDataID.cameraDepthAttachmentId, ShaderDataID.cameraDepthCopyTextureID);
    }

    void DrawTransparent(SortingSettings sortingSettings, DrawingSettings drawingSettings, FilteringSettings filteringSettings)
    {
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        context.DrawRenderers(
            renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        
        CopyCameraColor(transparentCmd);
        
        context.Submit();
    }
}