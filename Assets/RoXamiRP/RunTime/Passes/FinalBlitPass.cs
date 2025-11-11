using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class FinalBlitPass : RoXamiRenderPass
    {
        public FinalBlitPass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
        }
        
        const string bufferName = "FinalBlitPass";
        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName,
        };

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
            
            DrawDontCareDontCare(cmd, 
                renderingData.renderer.GetCameraColorBufferRT(), 
                //========================================
                //to
                renderingData.runtimeData.isCameraStackFinally? 
                    BuiltinRenderTextureType.CameraTarget : 
                    renderingData.renderer.GetSwitchCameraColorBufferRT(), 
                //========================================
                renderingData.shaderAsset.blitFullScreenTriangleMaterial, 0);
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp(CommandBuffer commandBuffer)
        {
        }
    }
}