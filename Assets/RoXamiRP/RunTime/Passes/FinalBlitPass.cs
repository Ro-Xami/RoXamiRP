using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class FinalBlitPass : RoXamiRenderPass
    {
        public FinalBlitPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
        
        const string bufferName = "FinalBlitPass";
        private CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            cmd.SetGlobalTexture(ShaderDataID.postSource0Id, ShaderDataID.cameraColorAttachmentId);
            cmd.SetRenderTarget(
                BuiltinRenderTextureType.CameraTarget,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                ShaderDataID.cameraDepthAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            cmd.DrawProcedural(
                Matrix4x4.identity, renderingData.shaderAsset.postMaterial, (int)PostShaderPass.finalBlit,
                MeshTopology.Triangles, 3
            );
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }
    }
}