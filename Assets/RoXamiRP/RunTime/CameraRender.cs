using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public partial class CameraRender
{
    // Camera camera;
    // ScriptableRenderContext context;
    // const string bufferName = "RoXami Render";
    // private static RenderingData renderingData = new RenderingData();
    // //
    // // static readonly LightingPass LightingPass = new LightingPass();
    // // static readonly GBufferPass gBufferPass = new GBufferPass();
    // // static readonly DepthToPositionWSPass depthToPositionWSPass = new DepthToPositionWSPass();
    // // static readonly ScreenSpaceShadowsPass screenSpaceShadowsPass = new ScreenSpaceShadowsPass();
    // // static readonly DeferredPass deferredPass = new DeferredPass();
    // // static readonly ForwardPass forwardPass = new ForwardPass();
    // // static readonly PostPass postPass = new PostPass();
    //
    // static readonly int cameraDepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    // static readonly int cameraColorAttachmentId = Shader.PropertyToID("_CameraColorAttachment");
    // static readonly int cameraDepthCopyTextureID = Shader.PropertyToID("_CameraDepthTexture");
    // static readonly int cameraColorCopyTextureID = Shader.PropertyToID("_CameraColorTexture");
    // static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");
    //
    // readonly CommandBuffer cmd = new CommandBuffer
    // {
    //     name = bufferName,
    // };

    public void Render(
        ScriptableRenderContext scriptableRenderContext , Camera cameraIndex , 
        bool GPUInstancing , bool DynamicBatching , 
        ShadowSettings shadowSettings , RoXamiRendererAsset rendererAsset, bool HDR)
    {
        // context = scriptableRenderContext;
        // camera = cameraIndex;
        // bool isHDR = HDR && camera.allowHDR;
        //
        // PrepareBuffer();
        // PrepareForSceneWindow();
        // SetCommonData();
        // SetUpRenderingData(GPUInstancing , DynamicBatching , shadowSettings , rendererAsset , isHDR);
        // SetUpCameraColorDepthRT();
        //
        // cmd.BeginSample(SampleName);
        // ExecuteBuffer();
        //
        // // LightingPass.Setup(ref renderingData);
        //
        // context.SetupCameraProperties(camera);
        //
        // // gBufferPass.SetUp(renderingData);
        // // depthToPositionWSPass.SetUp(renderingData);
        // // screenSpaceShadowsPass.SetUp(renderingData);
        // // deferredPass.SetUp(renderingData);
        // // forwardPass.SetUp(renderingData);
        // //
        // // postPass.Setup(renderingData);
        // DrawUnsupportedShaders();
        // // if (postPass.IsActive)
        // // {
        // //     postPass.Render();
        // // }
        // DrawGizmos();
        // CleanUp();
        //
        // cmd.EndSample(SampleName);
        // ExecuteBuffer();
        // context.Submit();
    }

    // private void SetUpCameraColorDepthRT()
    // {
    //     RenderTextureDescriptor cameraColorDescriptor = 
    //         new RenderTextureDescriptor(renderingData.width, renderingData.height);
    //     cameraColorDescriptor.depthBufferBits = 0;
    //     cameraColorDescriptor.colorFormat =
    //         renderingData.isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
    //     FilterMode cameraColorFilterMode = FilterMode.Bilinear;
    //
    //     RenderTextureDescriptor cameraDepthDescriptor =
    //         new RenderTextureDescriptor(renderingData.width, renderingData.height);
    //     cameraDepthDescriptor.depthBufferBits = 32;
    //     cameraDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
    //     FilterMode cameraDepthFilterMode = FilterMode.Point;
    //     
    //     renderingData.cameraColorAttachmentId = cameraColorAttachmentId;
    //     renderingData.cameraDepthAttachmentId = cameraDepthAttachmentId;
    //     renderingData.cameraColorCopyTextureID = cameraColorCopyTextureID;
    //     renderingData.cameraDepthCopyTextureID = cameraDepthCopyTextureID;
    //     renderingData.cameraColorDescriptor =  cameraColorDescriptor;
    //     renderingData.cameraDepthDescriptor = cameraDepthDescriptor;
    //     renderingData.cameraColorFilterMode =  cameraColorFilterMode;
    //     renderingData.cameraDepthFilterMode =  cameraDepthFilterMode;
    //     
    //     cmd.GetTemporaryRT(renderingData.cameraColorAttachmentId, cameraColorDescriptor, cameraColorFilterMode);
    //     cmd.GetTemporaryRT(renderingData.cameraDepthAttachmentId, cameraDepthDescriptor, cameraDepthFilterMode);
    //     cmd.GetTemporaryRT(renderingData.cameraColorCopyTextureID, cameraColorDescriptor, cameraColorFilterMode);
    //     cmd.GetTemporaryRT(renderingData.cameraDepthCopyTextureID, cameraDepthDescriptor, cameraDepthFilterMode);
    // }
    //
    // void ExecuteBuffer()
    // {
    //     context.ExecuteCommandBuffer(cmd);
    //     cmd.Clear();
    // }
    //
    // void CleanUp()
    // {
    //     cmd.ReleaseTemporaryRT(renderingData.cameraColorAttachmentId);
    //     cmd.ReleaseTemporaryRT(renderingData.cameraDepthAttachmentId);
    //     cmd.ReleaseTemporaryRT(renderingData.cameraColorCopyTextureID);
    //     cmd.ReleaseTemporaryRT(renderingData.cameraDepthCopyTextureID);
    //     LightingPass.CleanUp();
    //     gBufferPass.CleanUp();
    //     depthToPositionWSPass.CleanUp();
    //     deferredPass.CleanUp();
    //     forwardPass.CleanUp();
    // }
    //
    // internal static void SortStable(List<RoXamiRenderPass> list)
    // {
    //     int j;
    //     for (int i = 1; i < list.Count; ++i)
    //     {
    //         RoXamiRenderPass curr = list[i];
    //
    //         j = i - 1;
    //         for (; j >= 0 && curr < list[j]; --j)
    //             list[j + 1] = list[j];
    //
    //         list[j + 1] = curr;
    //     }
    // }
    //
    // void SetUpRenderingData(bool GPUInstancing , bool DynamicBatching , ShadowSettings shadowSettings , RoXamiRendererAsset rendererAsset, bool isHDR)
    // {
    //     camera.TryGetCullingParameters(out ScriptableCullingParameters p);
    //     p.shadowDistance = Mathf.Min(shadowSettings.maxDistance , camera.farClipPlane);
    //     CullingResults cullingResults = context.Cull(ref p);
    //
    //     renderingData.camera = camera;
    //     renderingData.context = context;
    //     renderingData.cullingResults = cullingResults;
    //     renderingData.width = camera.pixelWidth;
    //     renderingData.height = camera.pixelHeight;
    //     renderingData.isGPUInstancing = GPUInstancing;
    //     renderingData.isDynamicBatching = DynamicBatching;
    //     renderingData.isHDR = isHDR;
    //     renderingData.shadowSettings = shadowSettings;
    //     renderingData.RendererAsset = rendererAsset;
    // }
    //
    // void SetCommonData()
    // {
    //     Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
    //     Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
    //
    //     Matrix4x4 vpMatrix = projMatrix * viewMatrix;
    //     Matrix4x4 invVP = vpMatrix.inverse;
    //
    //     cmd.SetGlobalMatrix(matrixInvVP_ID, invVP);
    //
    // }
}