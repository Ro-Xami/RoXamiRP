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

        private RenderingData renderingData;
        
        private readonly int smaaEdgeRTID = Shader.PropertyToID("_SmaaEdgeRT");
        private readonly int smaaFactorRTID = Shader.PropertyToID("_SmaaFactorRT");

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            renderingData = renderData;
            
            var aaSettings = renderingData.antialiasingSettings;
            AntialiasingMode aaMode = aaSettings.antialiasingMode;

            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            var shaderAsset = renderingData.shaderAsset;
            switch (aaMode)
            {
                case AntialiasingMode.FXAA_Quality:
                    DrawFXAAQuality(shaderAsset);
                    break;
                case AntialiasingMode.FXAA_Console:
                    DrawFXAAConsole(shaderAsset);
                    break;
                case AntialiasingMode.SMAA:
                    DrawSMAA(shaderAsset);
                    break;
                case AntialiasingMode.Original:
                    FinalBlit(shaderAsset);
                    break;
            }
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
            if (renderingData.antialiasingSettings.antialiasingMode == AntialiasingMode.SMAA)
            {
                cmd.ReleaseTemporaryRT(smaaEdgeRTID);
                cmd.ReleaseTemporaryRT(smaaFactorRTID);
            }
        }

        void DrawFXAAQuality(ShaderAsset shaderAsset)
        {
            Draw(ShaderDataID.cameraColorAttachmentId, BuiltinRenderTextureType.CameraTarget,
                shaderAsset.fxaaMaterial, 0);
        }
        
        void DrawFXAAConsole(ShaderAsset shaderAsset)
        {
            Draw(ShaderDataID.cameraColorAttachmentId, BuiltinRenderTextureType.CameraTarget,
                shaderAsset.fxaaMaterial, 1);
        }
        
        void DrawSMAA(ShaderAsset shaderAsset)
        {
            var cameraData = renderingData.cameraData;
            cmd.GetTemporaryRT(smaaEdgeRTID, 
                cameraData.cameraColorDescriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(smaaFactorRTID, 
                cameraData.cameraColorDescriptor, FilterMode.Bilinear);
            
            Draw(ShaderDataID.cameraColorAttachmentId, smaaEdgeRTID,
                shaderAsset.smaaMaterial, 0);
            
            Draw(smaaEdgeRTID, smaaFactorRTID,
                shaderAsset.smaaMaterial, 1);
            
            Draw(ShaderDataID.cameraColorAttachmentId, BuiltinRenderTextureType.CameraTarget,
                shaderAsset.smaaMaterial, 2);
        }

        void FinalBlit(ShaderAsset shaderAsset)
        {
            Draw(ShaderDataID.cameraColorAttachmentId, BuiltinRenderTextureType.CameraTarget,
                shaderAsset.blitFullScreenTriangleMaterial, 0);
        }

        void Draw(RenderTargetIdentifier from, RenderTargetIdentifier to, Material mat, int passIndex)
        {
            cmd.SetGlobalTexture(ShaderDataID.postSource0Id, from);
            
            cmd.SetRenderTarget(
                to,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                ShaderDataID.cameraDepthAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            
            cmd.DrawProcedural(
                Matrix4x4.identity, mat, passIndex,
                MeshTopology.Triangles, 3
            );
        }
    }
}