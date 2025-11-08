using UnityEngine.Rendering;

namespace RoXamiRP
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
              renderingData.renderer.GetCameraColorBufferRT(),
              renderingData.renderer.GetCameraColorCopyRT(),
              ShaderDataID.cameraColorCopyTextureID);
          
          cmd.EndSample(bufferName);
          ExecuteCommandBuffer(context, cmd);
        }
    }
}