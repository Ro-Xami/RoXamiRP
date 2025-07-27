using UnityEngine;
using UnityEngine.Rendering;

public class RenderingPrePasses : RoXamiRenderPass
{
    public RenderingPrePasses(RenderPassEvent evt)
    {
        renderPassEvent = evt;
    }

    //private CommandBuffer cmd;

    public override void SetUp(CommandBuffer buffer, ref RenderingData renderingData)
    {
        //cmd = buffer;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        context.SetupCameraProperties(renderingData.cameraData.camera);
        //SetUpCameraColorDepthRT(cmd, ref renderingData);
        //ExecuteCommandBuffer(context, cmd);
    }

    // public override void CleanUp()
    // {
    //     cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorAttachmentId);
    //     cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthAttachmentId);
    //     cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorCopyTextureID);
    //     cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthCopyTextureID);
    // }
    //
    // private void SetUpCameraColorDepthRT(CommandBuffer cmd, ref RenderingData renderingData)
    // {
    //     int width = renderingData.cameraData.width;
    //     int height = renderingData.cameraData.height;
    //     RenderTextureDescriptor cameraColorDescriptor = 
    //         new RenderTextureDescriptor(width, height);
    //     cameraColorDescriptor.depthBufferBits = 0;
    //     cameraColorDescriptor.colorFormat = renderingData.cameraData.camera.allowHDR ? 
    //         RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
    //     FilterMode cameraColorFilterMode = FilterMode.Bilinear;
    //
    //     RenderTextureDescriptor cameraDepthDescriptor =
    //         new RenderTextureDescriptor(width, height);
    //     cameraDepthDescriptor.depthBufferBits = 32;
    //     cameraDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
    //     FilterMode cameraDepthFilterMode = FilterMode.Point;
    //
    //     renderingData.cameraData.cameraColorDescriptor =  cameraColorDescriptor;
    //     renderingData.cameraData.cameraDepthDescriptor = cameraDepthDescriptor;
    //     renderingData.cameraData.cameraColorFilterMode =  cameraColorFilterMode;
    //     renderingData.cameraData.cameraDepthFilterMode =  cameraDepthFilterMode;
    //     
    //     cmd.GetTemporaryRT(ShaderDataID.cameraColorAttachmentId, cameraColorDescriptor, cameraColorFilterMode);
    //     cmd.GetTemporaryRT(ShaderDataID.cameraDepthAttachmentId, cameraDepthDescriptor, cameraDepthFilterMode);
    //     cmd.GetTemporaryRT(ShaderDataID.cameraColorCopyTextureID, cameraColorDescriptor, cameraColorFilterMode);
    //     cmd.GetTemporaryRT(ShaderDataID.cameraDepthCopyTextureID, cameraDepthDescriptor, cameraDepthFilterMode);
    // }
}