using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class ScreenSpacePlanarReflectionFeature : RoXamiRenderFeature
    {
        public ComputeShader ssprCompute;
        public float planeHeight = 0;
        private ScreenSpacePlanarReflectionPass pass;

        public override void Create()
        {
            if (!ssprCompute)
            {
                return;
            }
            pass = new ScreenSpacePlanarReflectionPass(RenderPassEvent.AfterRenderingSkybox, ssprCompute, planeHeight);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!IsGameOrSceneCamera(renderingData.cameraData.camera)) return;
#endif

            if (!ssprCompute)
            {
                return;
            }
            
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            pass?.Dispose();
        }

        private class ScreenSpacePlanarReflectionPass : RoXamiRenderPass
        {
            private readonly ComputeShader compute;
            private readonly float planeHeight;

            public ScreenSpacePlanarReflectionPass(RenderPassEvent evt, ComputeShader cs, float height)
            {
                renderPassEvent = evt;
                this.compute = cs;
                this.planeHeight = height;
                m_ProfilingSampler = new ProfilingSampler(bufferName);
            }

            const string bufferName = "RoXami SSPR Pass";
            private CommandBuffer cmd;
            RenderingData renderingData;

            const string ssprKernelName = "SSPRCompute";
            const string ssprRtName = "_SSPRTexture";
            static readonly int ssprRtID = Shader.PropertyToID(ssprRtName);
            RTHandle ssprTextureRT;
            //static readonly int heightBufferID = Shader.PropertyToID("_SSPRHeightBuffer");
            static readonly int texelSizeID = Shader.PropertyToID("_texelSize");
            static readonly int heightID = Shader.PropertyToID("_height");

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
            {
                renderingData = renderData;
                cmd = renderingData.commandBuffer;
                
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    var ssprDescriptor = renderingData.cameraData.cameraColorDescriptor;
                    ssprDescriptor.enableRandomWrite = true;
                    RoXamiRTHandlePool.GetRTHandleIfNeeded(
                        ref ssprTextureRT, ssprDescriptor, renderingData.cameraData.cameraColorFilterMode, ssprRtName);
                    // cmd.GetTemporaryRT(heightBufferID,
                    //     ssprDescriptor, renderingData.cameraData.cameraColorFilterMode);

                    cmd.SetRenderTarget(ssprTextureRT,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                    //cmd.ClearRenderTarget(true, true, Color.clear);

                    SSPRCompute();
                }

                ExecuteCommandBuffer(context, cmd);
            }

            public override void Dispose()
            {
                ssprTextureRT?.Release();
            }

            void SSPRCompute()
            {
                int ssprKernel = compute.FindKernel(ssprKernelName);

                int width = renderingData.cameraData.width;
                int height = renderingData.cameraData.height;

                cmd.SetComputeFloatParam(compute, heightID, planeHeight);
                cmd.SetComputeVectorParam(compute, texelSizeID,
                    new Vector4(width, height, 1 / (float)width, 1 / (float)height));
                cmd.SetComputeTextureParam(compute, ssprKernel,
                    ShaderDataID.cameraDepthCopyTextureID, renderingData.renderer.GetCameraDepthCopyRT());
                cmd.SetComputeTextureParam(compute, ssprKernel,
                    ShaderDataID.cameraColorCopyTextureID, ShaderDataID.cameraColorCopyTextureID);
                // cmd.SetComputeTextureParam(compute, ssprKernel,
                //     heightBufferID, heightBufferID);
                cmd.SetComputeTextureParam(compute, ssprKernel,
                    ssprRtID, ssprTextureRT);

                int threadGroupX = Mathf.CeilToInt(width / 8.0f);
                int threadGroupY = Mathf.CeilToInt(height / 8.0f);

                cmd.DispatchCompute(compute, ssprKernel, threadGroupX, threadGroupY, 1);
                //cmd.DispatchCompute(compute, holeKernel, threadGroupX, threadGroupY, 1);
                
                cmd.SetGlobalTexture(ssprRtID, ssprTextureRT);
            }
        }
        
    }
}