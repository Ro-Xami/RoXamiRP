using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class AntialiasingPass : RoXamiRenderPass
    {
        const string bufferName = "AntialiasingPass";
        public AntialiasingPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        private CommandBuffer cmd;
        private RenderingData renderingData;

        private const string smaaEdgeRtName = "_SmaaEdgeRT";
        private const string smaaFactorRtName = "_SmaaFactorRT";
        static readonly int smaaFactorRtID = Shader.PropertyToID(smaaFactorRtName);
        private RTHandle smaaEdgeRT;
        private RTHandle smaaFactorRT;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            renderingData = renderData;
            
            var aaSettings = renderingData.antialiasingSettings;
            AntialiasingMode aaMode = aaSettings.antialiasingMode;

            cmd = renderingData.commandBuffer;
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
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
            }
            ExecuteCommandBuffer(context, cmd);
        }

        public override void Dispose()
        {
            smaaEdgeRT?.Release();
            smaaFactorRT?.Release();
        }

        void DrawFXAAQuality(ShaderAsset shaderAsset)
        {
            FinalDraw(shaderAsset.fxaaMaterial, 0);
        }
        
        void DrawFXAAConsole(ShaderAsset shaderAsset)
        {
            FinalDraw(shaderAsset.fxaaMaterial, 1);
        }
        
        void DrawSMAA(ShaderAsset shaderAsset)
        {
            var cameraData = renderingData.cameraData;
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref smaaEdgeRT, 
                cameraData.cameraColorDescriptor, FilterMode.Bilinear, smaaEdgeRtName);
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref smaaFactorRT, 
                cameraData.cameraColorDescriptor, FilterMode.Bilinear, smaaFactorRtName);
            
            Draw(renderingData.renderer.GetCameraColorBufferRT(), smaaEdgeRT,
                shaderAsset.smaaMaterial, 0);
            
            cmd.SetGlobalTexture(smaaFactorRtID, smaaFactorRT);
            Draw(smaaEdgeRT, smaaFactorRT,
                shaderAsset.smaaMaterial, 1);
            
            FinalDraw(shaderAsset.smaaMaterial, 2);
        }

        void FinalDraw(Material mat, int passIndex)
        {
            Draw(
                renderingData.renderer.GetCameraColorBufferRT(),
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