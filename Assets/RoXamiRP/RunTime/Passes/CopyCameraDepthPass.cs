using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class CopyCameraDepthPass : RoXamiRenderPass
    {
        const string bufferName = "RoXami Copy Depth";
        public CopyCameraDepthPass(RenderPassEvent evt)
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
                    renderingData.renderer.GetCameraDepthBufferRT(),
                    renderingData.renderer.GetCameraDepthCopyRT(),
                    ShaderDataID.cameraDepthCopyTextureID);
            }
            ExecuteCommandBuffer(context, cmd);
        }
    }
}