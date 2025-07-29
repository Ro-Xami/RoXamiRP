using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public partial class CameraRender
{
    Camera camera;
    ScriptableRenderContext context;
    const string bufferName = "RoXami Render";

    private RenderingData renderingData = new RenderingData();
    private readonly RoXamiRenderer renderer = new RoXamiRenderer();
    
    readonly CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };

    public void Render(
        ScriptableRenderContext scriptableRenderContext , Camera cameraIndex , 
        ShadowSettings shadowSettings , RoXamiRendererAsset rendererAsset, ShaderAsset shaderAsset)
    {
        context = scriptableRenderContext;
        camera = cameraIndex;
        
        PrepareBuffer();
        PrepareForSceneWindow();
        SetCommonData();
        
        SetUpRenderingData(shadowSettings , rendererAsset, shaderAsset);
        SetUpCameraColorDepthRT();
        
        renderer.AddFeatures(rendererAsset, ref renderingData);
        
        cmd.BeginSample(SampleName);
        ExecuteBuffer();
        
        renderer.CameraSetup(cmd, ref renderingData);
        renderer.ExecuteRoXamiRenderPass(context, ref renderingData);
        
        DrawUnsupportedShaders();
        DrawGizmos();
        
        CleanUp();
        
        cmd.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }
    
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    
    void CleanUp()
    {
        cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorAttachmentId);
        cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthAttachmentId);
        cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorCopyTextureID);
        cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthCopyTextureID);
        renderer.CameraCleanUp();
    }
    
    void SetUpRenderingData(ShadowSettings shadowSettings , RoXamiRendererAsset rendererAsset, ShaderAsset shaderAsset)
    {
        if (!camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            return;
        }

        p.shadowDistance = Mathf.Min(shadowSettings.maxDistance , camera.farClipPlane);
        CullingResults cullingResults = context.Cull(ref p);
    
        renderingData.shadowSettings = shadowSettings;
        renderingData.rendererAsset = rendererAsset;
        renderingData.cullingResults = cullingResults;
        renderingData.shaderAsset = shaderAsset;
        renderingData.cameraData.camera = camera;
        renderingData.cameraData.width = camera.pixelWidth;
        renderingData.cameraData.height = camera.pixelHeight;
    }
    
    private void SetUpCameraColorDepthRT()
    {
        int width = renderingData.cameraData.width;
        int height = renderingData.cameraData.height;
        RenderTextureDescriptor cameraColorDescriptor = 
            new RenderTextureDescriptor(width, height);
        cameraColorDescriptor.depthBufferBits = 0;
        cameraColorDescriptor.colorFormat = renderingData.rendererAsset.commonSettings.enableHDR ? 
            RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        FilterMode cameraColorFilterMode = FilterMode.Bilinear;
    
        RenderTextureDescriptor cameraDepthDescriptor =
            new RenderTextureDescriptor(width, height);
        cameraDepthDescriptor.depthBufferBits = 32;
        cameraDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
        FilterMode cameraDepthFilterMode = FilterMode.Point;

        renderingData.cameraData.cameraColorDescriptor =  cameraColorDescriptor;
        renderingData.cameraData.cameraDepthDescriptor = cameraDepthDescriptor;
        renderingData.cameraData.cameraColorFilterMode =  cameraColorFilterMode;
        renderingData.cameraData.cameraDepthFilterMode =  cameraDepthFilterMode;
        
        cmd.GetTemporaryRT(ShaderDataID.cameraColorAttachmentId, cameraColorDescriptor, cameraColorFilterMode);
        cmd.GetTemporaryRT(ShaderDataID.cameraDepthAttachmentId, cameraDepthDescriptor, cameraDepthFilterMode);
        cmd.GetTemporaryRT(ShaderDataID.cameraColorCopyTextureID, cameraColorDescriptor, cameraColorFilterMode);
        cmd.GetTemporaryRT(ShaderDataID.cameraDepthCopyTextureID, cameraDepthDescriptor, cameraDepthFilterMode);
    }

    
    void SetCommonData()
    {
        Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
        Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
    
        Matrix4x4 vpMatrix = projMatrix * viewMatrix;
        Matrix4x4 invVP = vpMatrix.inverse;
    
        cmd.SetGlobalMatrix(ShaderDataID.matrixInvVP_ID, invVP);
    }
}