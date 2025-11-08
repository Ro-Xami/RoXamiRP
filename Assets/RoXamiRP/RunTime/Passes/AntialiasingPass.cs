using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class AntialiasingPass : RoXamiRenderPass
    {
        public AntialiasingPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
        
        const string bufferName = "AntialiasingPass";
        private readonly CommandBuffer cmd = new CommandBuffer()
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
                case AntialiasingMode.None:
                    break;
                default:
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
            FinalDraw(renderingData.renderer.GetCameraColorBufferRT(), shaderAsset.fxaaMaterial, 0);
        }
        
        void DrawFXAAConsole(ShaderAsset shaderAsset)
        {
            FinalDraw(renderingData.renderer.GetCameraColorBufferRT(), shaderAsset.fxaaMaterial, 1);
        }
        
        void DrawSMAA(ShaderAsset shaderAsset)
        {
            var cameraData = renderingData.cameraData;
            cmd.GetTemporaryRT(smaaEdgeRTID, 
                cameraData.cameraColorDescriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(smaaFactorRTID, 
                cameraData.cameraColorDescriptor, FilterMode.Bilinear);
            
            Draw(renderingData.renderer.GetCameraColorBufferRT(), smaaEdgeRTID,
                shaderAsset.smaaMaterial, 0);
            
            Draw(smaaEdgeRTID, smaaFactorRTID,
                shaderAsset.smaaMaterial, 1);
            
            FinalDraw(renderingData.renderer.GetCameraColorBufferRT(), shaderAsset.smaaMaterial, 2);
        }

        void FinalDraw(RenderTargetIdentifier from, Material mat, int passIndex)
        {
            Draw(
                from,
                renderingData.runtimeData.isCameraStackFinally?
                    BuiltinRenderTextureType.CameraTarget:
                    renderingData.renderer.GetSwitchCameraColorBufferRT(),
                mat, passIndex);
        }

        void Draw(RenderTargetIdentifier from, RenderTargetIdentifier to, Material mat, int passIndex)
        {
            cmd.SetGlobalTexture(ShaderDataID.TempRtSource0ID, from);
            
            cmd.SetRenderTarget(
                to,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                renderingData.renderer.GetCameraDepthBufferRT(),
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            
            cmd.DrawProcedural(
                Matrix4x4.identity, mat, passIndex,
                MeshTopology.Triangles, 3
            );
        }
    }
}