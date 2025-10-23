using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class CopyCameraColorPass : RoXamiRenderPass
    {
        public CopyCameraColorPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
        
        const string bufferName = "RoXami Copy Color";

        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
          cmd.BeginSample(bufferName);
          ExecuteCommandBuffer(context, cmd);
          
          RoXamiRPCopyTexture(cmd, 
              ShaderDataID.cameraColorAttachmentId,
              ShaderDataID.cameraColorCopyTextureID);
          
          cmd.EndSample(bufferName);
          ExecuteCommandBuffer(context, cmd);
        }
    }
}