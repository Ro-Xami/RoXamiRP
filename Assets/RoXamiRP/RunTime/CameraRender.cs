using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public partial class CameraRender
{
    Camera camera;
    ScriptableRenderContext context;
    const string bufferName = "RoXami Render";
    private static RenderingData renderingData = new RenderingData();
    
    static readonly Lighting lighting = new Lighting();
    static readonly GBufferPass gBufferPass = new GBufferPass();
    static readonly DeferredPass deferredPass = new DeferredPass();
    static readonly ForwardPass forwardPass = new ForwardPass();
    static readonly PostPass postPass = new PostPass();
    
    static readonly int cameraDepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    static readonly int cameraColorAttachmentId = Shader.PropertyToID("_CameraColorAttachment");
    static readonly int cameraDepthCopyTextureID = Shader.PropertyToID("_CameraDepthTexture");
    static readonly int cameraColorCopyTextureID = Shader.PropertyToID("_CameraColorTexture");
    static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");

    readonly CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };

    public void Render(
        ScriptableRenderContext scriptableRenderContext , Camera cameraIndex , 
        bool GPUInstancing , bool DynamicBatching , 
        ShadowSettings shadowSettings , RoXamiRenderer renderer, bool HDR)
    {
        context = scriptableRenderContext;
        camera = cameraIndex;
        bool isHDR = HDR && camera.allowHDR;

        PrepareBuffer();
        PrepareForSceneWindow();
        SetCommonData();
        SetUpRenderingData(GPUInstancing , DynamicBatching , shadowSettings , renderer , isHDR);
        SetUpCameraColorDepthRT();
        
        cmd.BeginSample(SampleName);
        ExecuteBuffer();
        
        lighting.Setup(renderingData);
        
        context.SetupCameraProperties(camera);
        
        gBufferPass.SetUp(renderingData);
        deferredPass.SetUp(renderingData);
        forwardPass.SetUp(renderingData);

        postPass.Setup(renderingData);
        DrawUnsupportedShaders();
        if (postPass.IsActive)
        {
            postPass.Render();
        }
        DrawGizmos();
        CleanUp();
        
        cmd.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    private void SetUpCameraColorDepthRT()
    {
        RenderTextureDescriptor cameraColorDescriptor = 
            new RenderTextureDescriptor(renderingData.width, renderingData.height);
        cameraColorDescriptor.depthBufferBits = 0;
        cameraColorDescriptor.colorFormat =
            renderingData.isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        FilterMode cameraColorFilterMode = FilterMode.Bilinear;

        RenderTextureDescriptor cameraDepthDescriptor =
            new RenderTextureDescriptor(renderingData.width, renderingData.height);
        cameraDepthDescriptor.depthBufferBits = 32;
        cameraDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
        FilterMode cameraDepthFilterMode = FilterMode.Point;
        
        renderingData.cameraColorAttachmentId = cameraColorAttachmentId;
        renderingData.cameraDepthAttachmentId = cameraDepthAttachmentId;
        renderingData.cameraColorCopyTextureID = cameraColorCopyTextureID;
        renderingData.cameraDepthCopyTextureID = cameraDepthCopyTextureID;
        renderingData.cameraColorDescriptor =  cameraColorDescriptor;
        renderingData.cameraDepthDescriptor = cameraDepthDescriptor;
        renderingData.cameraColorFilterMode =  cameraColorFilterMode;
        renderingData.cameraDepthFilterMode =  cameraDepthFilterMode;
        
        cmd.GetTemporaryRT(renderingData.cameraColorAttachmentId, cameraColorDescriptor, cameraColorFilterMode);
        cmd.GetTemporaryRT(renderingData.cameraDepthAttachmentId, cameraDepthDescriptor, cameraDepthFilterMode);
        cmd.GetTemporaryRT(renderingData.cameraColorCopyTextureID, cameraColorDescriptor, cameraColorFilterMode);
        cmd.GetTemporaryRT(renderingData.cameraDepthCopyTextureID, cameraDepthDescriptor, cameraDepthFilterMode);
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    void CleanUp()
    {
        cmd.ReleaseTemporaryRT(renderingData.cameraColorAttachmentId);
        cmd.ReleaseTemporaryRT(renderingData.cameraDepthAttachmentId);
        cmd.ReleaseTemporaryRT(renderingData.cameraColorCopyTextureID);
        cmd.ReleaseTemporaryRT(renderingData.cameraDepthCopyTextureID);
        lighting.CleanUp();
        gBufferPass.CleanUp();
        deferredPass.CleanUp();
        forwardPass.CleanUp();
    }

    void SetUpRenderingData(bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRenderer renderer, bool isHDR)
    {
        camera.TryGetCullingParameters(out ScriptableCullingParameters p);
        p.shadowDistance = Mathf.Min(shadowSettings.maxDistance , camera.farClipPlane);
        CullingResults cullingResults = context.Cull(ref p);

        renderingData.camera = camera;
        renderingData.context = context;
        renderingData.cullingResults = cullingResults;
        renderingData.width = camera.pixelWidth;
        renderingData.height = camera.pixelHeight;
        renderingData.isGPUInstancing = GPUInstancing;
        renderingData.isDynamicBatching = DynamicBatching;
        renderingData.isHDR = isHDR;
        renderingData.shadowSettings = shadowSettings;
        renderingData.renderer = renderer;
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