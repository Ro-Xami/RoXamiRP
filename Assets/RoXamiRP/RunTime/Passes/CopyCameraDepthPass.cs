using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class CopyCameraDepthPass : RoXamiRenderPass
    {
        public CopyCameraDepthPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
        
        const string bufferName = "RoXami Copy Depth";

        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
          
            RoXamiRPCopyTexture(cmd, 
                ShaderDataID.cameraDepthAttachmentId,
                ShaderDataID.cameraDepthCopyTextureID);
          
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }
    }
}