using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class GlobalFogFeature : RoXamiRenderFeature
    {
        [SerializeField] RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

        private Material mat;
        const int passIndex = 0;
        private GlobalFogPass pass;
        
        public override void Create()
        {
            mat = CoreUtils.CreateEngineMaterial("");
            pass = new GlobalFogPass(renderPassEvent, mat, passIndex);
        }

        public override void AddRenderPasses(RoXamiRenderLoop renderLoop, ref RenderingData renderingData)
        {
            if (RoXamiFeatureManager.Instance.IsActive(RoXamiFeatureStack.GlobalFog))
            {
                renderLoop.EnqueuePass(pass);
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
            public GlobalFogPass(RenderPassEvent evt, Material mMat, int mPassIndex)
            {
                renderPassEvent = evt;
                this.m_Mat = mMat;
                this.m_PassIndex = mPassIndex;
            }
            
            const string bufferName = "RoXami GlobalFog";
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
                
                DrawFullScreenTriangles(cmd, m_Mat, m_PassIndex);
                
                cmd.EndSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
            }

            public override void CleanUp()
            {
                
            }
        }
    }
}