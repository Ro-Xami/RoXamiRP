using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GBufferPass
{
    const string bufferName = "RoXami GBuffer";

    private readonly CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };
    
    static readonly ShaderTagId toonGBufferShaderTagId = new ShaderTagId("ToonGBuffer");
    
    static readonly int[] gBufferNameIDs = new int[]
    {
        Shader.PropertyToID("_GBuffer0"),
        Shader.PropertyToID("_GBuffer1"),
        Shader.PropertyToID("_GBuffer2"),
        Shader.PropertyToID("_GBuffer3"),
    };

    private static readonly RenderTargetIdentifier[] gBufferTargets =  new RenderTargetIdentifier[]
    {
        new RenderTargetIdentifier(gBufferNameIDs[0]),
        new RenderTargetIdentifier(gBufferNameIDs[1]),
        new RenderTargetIdentifier(gBufferNameIDs[2]),
        new RenderTargetIdentifier(gBufferNameIDs[3]),
    };
    
    static readonly DepthToPositionWS depthToPositionWS = new DepthToPositionWS();
    
    RenderingData renderingData;
    
     public void SetUp(RenderingData renderData)
    {
        renderingData = renderData;
        GetGBufferRT();

        ClearCmdRenderTarget();
        cmd.BeginSample(bufferName);
        ExecuteBuffer();
        
        SetDrawingSettings(out var drawingSettings, out var filteringSettings);
        renderingData.context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        renderingData.context.Submit();
        
        CopyCameraDepth();
        depthToPositionWS.CalculatePositionWS(cmd, renderData);
        
        cmd.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void CopyCameraDepth()
    {
        cmd.CopyTexture(renderingData.cameraDepthAttachmentId, renderingData.cameraDepthCopyTextureID);
    }

    private void GetGBufferRT()
    {
        int width = renderingData.width;
        int height = renderingData.height;

        cmd.GetTemporaryRT(gBufferNameIDs[0], renderingData.cameraColorDescriptor, renderingData.cameraColorFilterMode);    //Albedo
        cmd.GetTemporaryRT(gBufferNameIDs[1], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);   //normal
        cmd.GetTemporaryRT(gBufferNameIDs[2], width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);  //Metallic/Roughness/Ao
        cmd.GetTemporaryRT(gBufferNameIDs[3], renderingData.cameraColorDescriptor, FilterMode.Point);   //Emission
    }

    void ClearCmdRenderTarget()
    {
        cmd.SetRenderTarget(gBufferTargets, renderingData.cameraDepthAttachmentId);
        
        CameraClearFlags flags = renderingData.camera.clearFlags;
        cmd.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                renderingData.camera.backgroundColor.linear : Color.clear
        );
    }

    void SetDrawingSettings(out DrawingSettings drawingSettings, out FilteringSettings filteringSettings)
    {
        SortingSettings sortingSettings = new SortingSettings(renderingData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        drawingSettings = new DrawingSettings(toonGBufferShaderTagId, sortingSettings) { 
            enableDynamicBatching = renderingData.isDynamicBatching , 
            enableInstancing = renderingData.isGPUInstancing , 
            perObjectData = 
                PerObjectData.ReflectionProbes |
                PerObjectData.LightProbe};

        filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
    }
    
    private void ExecuteBuffer()
    {
        renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        foreach (var gBufferID in gBufferNameIDs)
        {
            cmd.ReleaseTemporaryRT(gBufferID);
        }
        
        depthToPositionWS.CleanUp(cmd);
    }
}