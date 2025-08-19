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
            var aaSettings = renderingData.antialiasingSettings;
            AntialiasingMode aaMode = aaSettings.antialiasingMode;

            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            cmd.SetGlobalTexture(ShaderDataID.postSource0Id, ShaderDataID.cameraColorAttachmentId);
            cmd.SetRenderTarget(
                BuiltinRenderTextureType.CameraTarget,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                ShaderDataID.cameraDepthAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);

            var shaderAsset = renderingData.shaderAsset;
            switch (aaMode)
            {
                case AntialiasingMode.FXAA_Quality:
                    DrawFXAAQuality(shaderAsset);
                    break;
                case AntialiasingMode.FXAA_Console:
                    DrawFXAAConsole(shaderAsset);
                    break;
                case AntialiasingMode.Original:
                    FinalBlit(shaderAsset);
                    break;
            }
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        void DrawFXAAQuality(ShaderAsset shaderAsset)
        {
            Draw(shaderAsset.fxaaMaterial, 0);
        }
        
        void DrawFXAAConsole(ShaderAsset shaderAsset)
        {
            Draw(shaderAsset.fxaaMaterial, 1);
        }

        void FinalBlit(ShaderAsset shaderAsset)
        {
            Draw(shaderAsset.blitFullScreenTriangleMaterial, 0);
        }

        void Draw(Material mat, int passIndex)
        {
            cmd.DrawProcedural(
                Matrix4x4.identity, mat, passIndex,
                MeshTopology.Triangles, 3
            );
        }
    }
}