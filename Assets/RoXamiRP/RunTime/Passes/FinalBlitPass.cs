using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
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
                ShaderDataID.cameraColorAttachmentId, 
                //========================================
                //to
                renderingData.runtimeData.isFinalBlit? 
                    BuiltinRenderTextureType.CameraTarget : 
                ShaderDataID.cameraColorAttachmentId = ShaderDataID.cameraColorAttachmentId == ShaderDataID.cameraColorAttachmentAId?
                    ShaderDataID.cameraColorAttachmentBId: ShaderDataID.cameraColorAttachmentAId, 
                //========================================
                renderingData.shaderAsset.blitFullScreenTriangleMaterial, 0);
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
        }
    }
}