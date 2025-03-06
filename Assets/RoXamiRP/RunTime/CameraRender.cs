using System;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRender
{
    Camera camera;
    ScriptableRenderContext context;
    const string bufferName = "RoXami Render";
    CullingResults cullingResults;

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");

    Lighting lighting = new Lighting();

    CommandBuffer commandBuffer = new CommandBuffer
    {
        name = bufferName,

    };

    public void Render(ScriptableRenderContext context , Camera camera , bool GPUInstancing , bool DynamicBatching)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();

        if (!Cull())
        {
            return;
        }

        SetUp();

        lighting.Setup(context , cullingResults);

        DrawGeometry(GPUInstancing, DynamicBatching);

        DrawUnsupportedShaders();
        DrawGizmos();

        Submit();
    }

    void SetUp()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;

        commandBuffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
            camera.backgroundColor.linear : Color.clear);

        commandBuffer.BeginSample(SampleName);
        ExcuteBuffer();        
    }

    void DrawGeometry(bool GPUInstancing, bool DynamicBatching)
    {
        SortingSettings sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        { enableDynamicBatching = DynamicBatching , enableInstancing = GPUInstancing };
        drawingSettings.SetShaderPassName(1 , toonLitShaderTagId);

        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Submit()
    {
        commandBuffer.EndSample(SampleName);
        ExcuteBuffer();
        context.Submit();
    }

    void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
}