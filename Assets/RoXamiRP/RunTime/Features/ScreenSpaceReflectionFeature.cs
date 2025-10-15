using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class ScreenSpaceReflectionFeature : RoXamiRenderFeature
    {
        [SerializeField] public SsrSettings settings;

        [Serializable]
        public class SsrSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingDeferredLights;
            public ComputeShader ComputeShader;
            public int maxStep = 20;
            public float stepSize = 0.1f;
        }

        private SsrPass ssr;
        
        public override void Create()
        {
            if (settings == null)
            {
                return;
            }

            ssr = new SsrPass(settings);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
            //&& RoXamiFeatureManager.Instance.IsActive(RoXamiFeatureStack.ScreenSpaceReflectionFeature)
            if (ssr != null)
            {
                renderer.EnqueuePass(ssr);
            }
        }
        
        private class SsrPass : RoXamiRenderPass
        {
            private readonly SsrSettings settings;
            private readonly int kernel;
            private const string bufferName = "ScreenSpaceReflection";
            public SsrPass(SsrSettings settings)
            {
                if (settings == null || settings.ComputeShader == null)
                {
                    return;
                }
                
                this.renderPassEvent = settings.renderPassEvent;
                this.settings = settings;
                kernel = settings.ComputeShader.FindKernel(bufferName);
            }

            private readonly CommandBuffer cmd = new CommandBuffer()
            {
                name = bufferName,
            };
            
            private readonly int 
                ssrTextureID = Shader.PropertyToID("_ScreenSpaceReflectionTexture"),
                maxStepID = Shader.PropertyToID("_maxStep"),
                stepSizeID = Shader.PropertyToID("_stepSize"),
                texelSizeID = Shader.PropertyToID("_texelSize");

            public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (settings == null || settings.ComputeShader == null || kernel < 0 || 
                    !renderingData.rendererAsset.rendererSettings.enableDeferredRendering)
                {
                    return;
                }
                
                cmd.BeginSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
                
                GetSetClearRtTarget(renderingData, out int width, out int height);

                Draw(width, height);

                cmd.EndSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
            }

            private void Draw(int width, int height)
            {
                var cs = settings.ComputeShader;
                cmd.SetComputeIntParam(cs, maxStepID, settings.maxStep);
                cmd.SetComputeFloatParam(cs, stepSizeID, settings.stepSize);
                cmd.SetComputeVectorParam(cs, texelSizeID,
                    new Vector4(width, height, 1 / (float)width, 1 / (float)height));
                
                cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.cameraColorAttachmentId, ShaderDataID.cameraColorAttachmentId);
                cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.cameraDepthAttachmentId, ShaderDataID.cameraDepthAttachmentId);
                cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal], ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal]);
                cmd.SetComputeTextureParam(cs, kernel, ssrTextureID, ssrTextureID);
                
                int threadGroupX = Mathf.CeilToInt(width / 8.0f);
                int threadGroupY = Mathf.CeilToInt(height / 8.0f);
                cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
            }

            private void GetSetClearRtTarget(RenderingData renderingData,  out int width, out int height)
            {
                var descriptor = renderingData.cameraData.cameraColorDescriptor;
                width = descriptor.width;
                height =descriptor.height;
                
                cmd.GetTemporaryRT(ssrTextureID, 
                    width, height, descriptor.depthBufferBits,
                    FilterMode.Bilinear, descriptor.colorFormat, RenderTextureReadWrite.Linear, 
                    0, true);
                
                cmd.SetRenderTarget(ssrTextureID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                cmd.ClearRenderTarget(true, true, Color.clear);
            }

            public override void CleanUp()
            {
                cmd.ReleaseTemporaryRT(ssrTextureID);
            }
        }
    }
}