using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class CopyCameraColorPass : RoXamiRenderPass
    {
        const string bufferName = "RoXami Copy Color";
        public CopyCameraColorPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        private CommandBuffer cmd;
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
          cmd = renderingData.commandBuffer;
          using (new ProfilingScope(cmd, m_ProfilingSampler))
          {
              RoXamiRPCopyTexture(cmd, 
                  renderingData.renderer.GetCameraColorBufferRT(),
                  renderingData.renderer.GetCameraColorCopyRT(),
                  ShaderDataID.cameraColorCopyTextureID);
          }
          ExecuteCommandBuffer(context, cmd);
        }
    }
}