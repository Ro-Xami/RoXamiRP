using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class GlobalFogFeature : RoXamiRenderFeature
    {
        [SerializeField] RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        private Material m_Mat;
        private Material mat
        {
            get
            {
                if (!m_Mat)
                {
                    m_Mat = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/FullScreenGlobalFog");
                }
                return m_Mat;
            }
        }
        
        const int passIndex = 0;
        private GlobalFogPass pass;
        
        public override void Create()
        {
            pass = new GlobalFogPass(renderPassEvent, mat, passIndex);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!IsGameOrSceneCamera(renderingData.cameraData.camera)) return;
#endif
            
            if (RoXamiFeatureManager.Instance.IsActive(RoXamiFeatureStack.GlobalFog))
            {
                renderer.EnqueuePass(pass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(mat);
        }

        private class GlobalFogPass : RoXamiRenderPass
        {
            private readonly Material m_Mat;
            private readonly int m_PassIndex;
            const string bufferName = "RoXami GlobalFog";
            
            public GlobalFogPass(RenderPassEvent evt, Material mMat, int mPassIndex)
            {
                renderPassEvent = evt;
                this.m_Mat = mMat;
                this.m_PassIndex = mPassIndex;
                m_ProfilingSampler = new ProfilingSampler(bufferName);
            }

            private CommandBuffer cmd;

            public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!m_Mat)
                {
                    return;
                }

                cmd = renderingData.commandBuffer;
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    cmd.SetGlobalTexture(
                        ShaderDataID.cameraDepthCopyTextureID, 
                        renderingData.renderer.GetCameraDepthBufferRT());
                
                    cmd.SetRenderTarget(
                        renderingData.renderer.GetCameraColorBufferRT(), 
                        RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
                
                    DrawFullScreenTriangles(cmd, m_Mat, m_PassIndex);
                    
                }
                ExecuteCommandBuffer(context, cmd);
                
            }
            
        }
    }
}