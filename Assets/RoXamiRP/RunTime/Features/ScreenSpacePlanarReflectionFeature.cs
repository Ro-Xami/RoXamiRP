using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    [Serializable]

    public class ScreenSpacePlanarReflectionFeature : RoXamiRenderFeature
    {
        private class ScreenSpacePlanarReflectionPass : RoXamiRenderPass
        {
            private readonly ComputeShader compute;
            private readonly float planeHeight;

            public ScreenSpacePlanarReflectionPass(RenderPassEvent evt, ComputeShader cs, float height)
            {
                renderPassEvent = evt;
                this.compute = cs;
                this.planeHeight = height;
            }

            const string bufferName = "RoXami SSPR Pass";

            private readonly CommandBuffer cmd = new CommandBuffer()
            {
                name = bufferName
            };

            RenderingData renderingData;


            const string ssprKernelName = "SSPRCompute";

            //const string holeKernelName = "SSPRHole";
            static readonly int ssprTextureID = Shader.PropertyToID("_SSPRTexture");
            static readonly int heightBufferID = Shader.PropertyToID("_SSPRHeightBuffer");
            static readonly int texelSizeID = Shader.PropertyToID("_texelSize");
            static readonly int heightID = Shader.PropertyToID("_height");

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
            {
                renderingData = renderData;

                var ssprDescriptor = renderingData.cameraData.cameraColorDescriptor;
                ssprDescriptor.enableRandomWrite = true;
                cmd.GetTemporaryRT(ssprTextureID,
                    ssprDescriptor, renderingData.cameraData.cameraColorFilterMode);
                cmd.GetTemporaryRT(heightBufferID,
                    ssprDescriptor, renderingData.cameraData.cameraColorFilterMode);

                cmd.SetRenderTarget(ssprTextureID,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                //cmd.ClearRenderTarget(true, true, Color.clear);
                
                cmd.BeginSample(bufferName);
                ExecuteBuffer(context);
                SSPRCompute();
                cmd.EndSample(bufferName);
                ExecuteBuffer(context);
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
                    ShaderDataID.cameraDepthCopyTextureID, ShaderDataID.cameraDepthCopyTextureID);
                cmd.SetComputeTextureParam(compute, ssprKernel,
                    ShaderDataID.cameraColorCopyTextureID, ShaderDataID.cameraColorCopyTextureID);
                cmd.SetComputeTextureParam(compute, ssprKernel,
                    heightBufferID, heightBufferID);
                cmd.SetComputeTextureParam(compute, ssprKernel,
                    ssprTextureID, ssprTextureID);

                int threadGroupX = Mathf.CeilToInt(width / 8.0f);
                int threadGroupY = Mathf.CeilToInt(height / 8.0f);

                cmd.DispatchCompute(compute, ssprKernel, threadGroupX, threadGroupY, 1);
                //cmd.DispatchCompute(compute, holeKernel, threadGroupX, threadGroupY, 1);
            }

            public override void CleanUp()
            {
                cmd.ReleaseTemporaryRT(ssprTextureID);
                cmd.ReleaseTemporaryRT(heightBufferID);
            }

            void ExecuteBuffer(ScriptableRenderContext context)
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
        }

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

        public override void AddRenderPasses(RoXamiRenderLoop renderLoop, ref RenderingData renderingData)
        {
            if (!ssprCompute)
            {
                return;
            }
            
            renderLoop.EnqueuePass(pass);
        }
    }
}