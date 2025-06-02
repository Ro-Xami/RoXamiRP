using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public partial class CameraRender
{
    Camera camera;
    ScriptableRenderContext context;
    const string bufferName = "RoXami Render";

    static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");
    
    static readonly Lighting lighting = new Lighting();
    
    static RoXamiPost post = new RoXamiPost();
    static readonly int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");

    readonly CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };

    public static RenderingData renderingData;
    
    public struct RenderingData
    {
        public int width;
        public int height;
        public CullingResults cullingResults;
        public RenderTargetIdentifier colorRT;
        public RenderTargetIdentifier depthRT;
    }

    public void Render(ScriptableRenderContext context , Camera camera , bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRenderer renderer )
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();

        renderingData = GetRenderingData(shadowSettings.maxDistance);

        cmd.BeginSample(SampleName);
        ExcuteBuffer();
        lighting.Setup(context , renderingData.cullingResults , shadowSettings);
        cmd.EndSample(SampleName);
        
        cmd.BeginSample(SampleName);
        SetUp();
        post.Setup(context,camera,renderer);
        ExcuteBuffer();
        
        DrawGeometry(GPUInstancing, DynamicBatching);
        DrawUnsupportedShaders();
        if (post.IsActive)
        {
            post.Render(frameBufferId);
        }
        DrawGizmos();
        
        CleanUp();
        cmd.EndSample(SampleName);
        Submit();
    }

    void SetUp()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        
        cmd.GetTemporaryRT(
            frameBufferId, renderingData.width, renderingData.height,
            32, FilterMode.Bilinear, RenderTextureFormat.Default
        );
        cmd.SetRenderTarget(
            frameBufferId,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );

        cmd.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear
        );
    }

    void DrawGeometry(bool GPUInstancing, bool DynamicBatching)
    {
        SortingSettings sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { 
            enableDynamicBatching = DynamicBatching , 
            enableInstancing = GPUInstancing , 
            perObjectData = 
                PerObjectData.ReflectionProbes |
                PerObjectData.LightProbe};
        drawingSettings.SetShaderPassName(1 , toonLitShaderTagId);

        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        ExcuteBuffer();

        context.DrawSkybox(camera);
        
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Submit()
    {
        ExcuteBuffer();
        context.Submit();
    }

    void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    void CleanUp()
    {
        lighting.CleanUp();

        cmd.ReleaseTemporaryRT(frameBufferId);
    }

    RenderingData GetRenderingData(float maxShadowDistance)
    {
        camera.TryGetCullingParameters(out ScriptableCullingParameters p);
        p.shadowDistance = Mathf.Min(maxShadowDistance , camera.farClipPlane);
        CullingResults cullingResults = context.Cull(ref p);
        
        RenderingData data = new RenderingData();
        data.width = camera.pixelWidth;
        data.height = camera.pixelHeight;
        data.cullingResults = cullingResults;
        // data.colorRT = camerColorRT;
        // data.depthRT = cameDepthRT;
        
        return data;
    }
}