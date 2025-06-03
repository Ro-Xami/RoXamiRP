using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GBufferPass
{
    const string bufferName = "RoXamiGBuffer";

    private CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };
    
    static readonly ShaderTagId toonGBufferShaderTagId = new ShaderTagId("ToonGBuffer");
    
    static int[] GbufferNameIds = new int[]
    {
        Shader.PropertyToID("Gbuffer0"),
        Shader.PropertyToID("Gbuffer1"),
        Shader.PropertyToID("Gbuffer2"),
        Shader.PropertyToID("Gbuffer3"),
    };
    
    public static RenderTargetIdentifier[] GbufferIds = new RenderTargetIdentifier[]
    {
        new RenderTargetIdentifier(GbufferNameIds[0]),
        new RenderTargetIdentifier(GbufferNameIds[1]),
        new RenderTargetIdentifier(GbufferNameIds[2]),
        new RenderTargetIdentifier(GbufferNameIds[3])
    };

    //private static readonly int cameraColorRTID = Shader.PropertyToID("CameraColorAttachment");
    private static readonly int camerDepthRTID = Shader.PropertyToID("CameraDepthAttachment");
    //private readonly RenderTargetIdentifier camerColorRT = new RenderTargetIdentifier(cameraColorRTID);
    private readonly RenderTargetIdentifier cameDepthRT = new RenderTargetIdentifier(camerDepthRTID);
    
     public void SetUp()
    {
        GetGbufferRT();

        cmd.BeginSample(bufferName);
        cmd.SetRenderTarget(GbufferIds, cameDepthRT);
        ClearCmdRenderTarget();
        ExecuteBuffer();
        
        SetDrawingSettings(out var drawingSettings, out var filteringSettings);
        CameraRender.renderingData.context.DrawRenderers(CameraRender.renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        
        cmd.EndSample(bufferName);
        CameraRender.renderingData.context.Submit();
    }

    private void GetGbufferRT()
    {
        // cmd.GetTemporaryRT(cameraColorRTID, CameraRender.renderingData.width, CameraRender.renderingData.height, 24,
        //     FilterMode.Point, GraphicsFormat.B10G11R11_UFloatPack32);

        // cmd.SetRenderTarget(camerColorRT, RenderBufferLoadAction.DontCare,
        //     RenderBufferStoreAction.Store,
        //     camerDepthRTID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        //cmd.ClearRenderTarget(true , true , Color.clear);


        RenderTextureDescriptor gbufferdesc =
            new RenderTextureDescriptor(CameraRender.renderingData.width, CameraRender.renderingData.height, RenderTextureFormat.ARGB32);
        // gbufferdesc.depthBufferBits = 0; 
         //gbufferdesc.stencilFormat = GraphicsFormat.; 
        // gbufferdesc.graphicsFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
        //     ? GraphicsFormat.R8G8B8A8_SRGB
        //     : GraphicsFormat.R8G8B8A8_UNorm; 
        
        cmd.GetTemporaryRT(camerDepthRTID, CameraRender.renderingData.width, CameraRender.renderingData.height, 24,
            FilterMode.Point,
            RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        
        int width = CameraRender.renderingData.width;
        int height = CameraRender.renderingData.height;
        RenderTextureFormat hdrFormat = CameraRender.renderingData.isHDR?RenderTextureFormat.Default:RenderTextureFormat.DefaultHDR;
        RenderTextureFormat format = RenderTextureFormat.Default;
        FilterMode filterMode = FilterMode.Bilinear;
        //GraphicsFormat graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;

        cmd.GetTemporaryRT(GbufferNameIds[0], width, height, 0, filterMode, hdrFormat); //Albedo
        cmd.GetTemporaryRT(GbufferNameIds[1], width, height, 0, filterMode, format); //normal
        cmd.GetTemporaryRT(GbufferNameIds[2], width, height, 0, filterMode, format); //Metallic/Roughness/Ao
        //gbufferdesc.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        cmd.GetTemporaryRT(GbufferNameIds[3], width, height, 0, filterMode, hdrFormat); //Emission
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

    void SetDrawingSettings(out DrawingSettings drawingSettings, out FilteringSettings filteringSettings)
    {
        SortingSettings sortingSettings = new SortingSettings(CameraRender.renderingData.camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        drawingSettings = new DrawingSettings(toonGBufferShaderTagId, sortingSettings) { 
            enableDynamicBatching = CameraRender.renderingData.isDynamicBatching , 
            enableInstancing = CameraRender.renderingData.isGPUInstancing , 
            perObjectData = 
                PerObjectData.ReflectionProbes |
                PerObjectData.LightProbe};

        filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
    }
    
    private void ExecuteBuffer()
    {
        CameraRender.renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        for (int i = 0; i < GbufferNameIds.Length; i++)
        {
            cmd.ReleaseTemporaryRT(GbufferNameIds[i]);
        }
        cmd.ReleaseTemporaryRT(camerDepthRTID);
    }
}