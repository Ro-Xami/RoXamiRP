using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public partial class CameraRender
{
    Camera camera;
    ScriptableRenderContext context;
    const string bufferName = "RoXami Render";
    public static RenderingData renderingData;
    
    static readonly Lighting lighting = new Lighting();
    static readonly ForwardPass forwardPass = new ForwardPass();
    static readonly PostPass postPass = new PostPass();
    
    static readonly int cameraDepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    static readonly int cameraColorAttachmentId = Shader.PropertyToID("_CameraColorAttachment");
    static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");

    readonly CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };

    public struct RenderingData
    {
        public int width;
        public int height;
        public Camera camera;
        public ScriptableRenderContext context;
        public CullingResults cullingResults;
        public RoXamiRenderer renderer;
        public ShadowSettings shadowSettings;
        public bool isGPUInstancing;
        public bool isDynamicBatching;
        public bool isHDR;
        public int cameraDepthBufferId;
        public int cameraColorBufferId;
    }

    public void Render(ScriptableRenderContext context , Camera camera , bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRenderer renderer, bool HDR)
    {
        this.context = context;
        this.camera = camera;
        bool isHDR = HDR && camera.allowHDR;

        PrepareBuffer();
        PrepareForSceneWindow();
        SetCommonData();

        renderingData = GetRenderingData(shadowSettings.maxDistance , GPUInstancing , DynamicBatching , shadowSettings , renderer , isHDR);

        cmd.BeginSample(SampleName);
        ExcuteBuffer();
        
        lighting.Setup(context , renderingData.cullingResults , shadowSettings);
        
        context.SetupCameraProperties(camera);
        forwardPass.Render();

        postPass.Setup(context, camera, renderer, isHDR);
        DrawUnsupportedShaders();
        if (postPass.IsActive)
        {
            postPass.Render();
        }
        DrawGizmos();
        CleanUp();
        
        cmd.EndSample(SampleName);
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
        forwardPass.CleanUp();
    }

    RenderingData GetRenderingData(float maxShadowDistance, bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRenderer renderer, bool isHDR)
    {
        camera.TryGetCullingParameters(out ScriptableCullingParameters p);
        p.shadowDistance = Mathf.Min(maxShadowDistance , camera.farClipPlane);
        CullingResults cullingResults = context.Cull(ref p);
        
        RenderingData data = new RenderingData();
        data.camera = camera;
        data.context = context;
        data.cullingResults = cullingResults;
        data.width = camera.pixelWidth;
        data.height = camera.pixelHeight;
        data.isGPUInstancing = GPUInstancing;
        data.isDynamicBatching = DynamicBatching;
        data.isHDR = isHDR;
        data.shadowSettings = shadowSettings;
        data.renderer = renderer;
        data.cameraColorBufferId = cameraColorAttachmentId;
        data.cameraDepthBufferId = cameraDepthAttachmentId;
        
        return data;
    }

    void SetCommonData()
    {
        Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
        Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);

        Matrix4x4 vpMatrix = projMatrix * viewMatrix;
        Matrix4x4 invVP = vpMatrix.inverse;

        cmd.SetGlobalMatrix(matrixInvVP_ID, invVP);

    }
}