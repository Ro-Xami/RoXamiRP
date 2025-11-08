using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class ScreenSpaceReflectionFeature : RoXamiRenderFeature
    {
        [SerializeField] public SsrSettings settings;

        [Serializable]
        public class SsrSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingDeferredDiffuse;
            public ComputeShader computeShader;
            public int maxStep = 20;
            public float stepSize = 1f;
            public float thickness = 1f;
        }

        private SsrPass ssr;
        
        public override void Create()
        {
            if (settings == null)
            {
                return;
            }

            if (ssr == null)
            {
                ssr = new SsrPass(settings);
            }
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
            //&& RoXamiFeatureManager.Instance.IsActive(RoXamiFeatureStack.ScreenSpaceReflectionFeature)
            if (ssr != null)
            {
                renderer.EnqueuePass(ssr);
            }
        }

        protected override void Dispose(bool disposing)
        {
            ssr?.Dispose();
        }

        private class SsrPass : RoXamiRenderPass
        {
            private readonly SsrSettings settings;
            private readonly int kernel;
            private const string bufferName = "ScreenSpaceReflection";
            public SsrPass(SsrSettings settings)
            {
                if (settings == null || !settings.computeShader)
                {
                    return;
                }
                
                this.renderPassEvent = settings.renderPassEvent;
                this.settings = settings;
                kernel = settings.computeShader.FindKernel(bufferName);
            }

            private readonly CommandBuffer cmd = new CommandBuffer()
            {
                name = bufferName,
            };

            private RTHandle ssrRT;
            
            private readonly int 
                ssrTextureID = Shader.PropertyToID("_ScreenSpaceReflectionTexture"),
                ssrParamsID = Shader.PropertyToID("_ssrParams"),
                texelSizeID = Shader.PropertyToID("_texelSize");

            public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var cameraData = renderingData.cameraData;
                var descriptor = renderingData.cameraData.cameraColorDescriptor;
                descriptor.enableRandomWrite = true;
                descriptor.useMipMap = true;
                descriptor.autoGenerateMips = true;
                descriptor.mipCount = 7;

                RoXamiRTHandlePool.GetRTHandleIfNeeded(ref ssrRT, descriptor, name:"ScreenSpaceReflectionTexture", FilterMode.Bilinear);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (settings == null || !settings.computeShader || kernel < 0)
                {
                    cmd.DisableShaderKeyword(ShaderDataID.enableScreenSpaceReflectionID);
                    return;
                }
                
                cmd.EnableShaderKeyword(ShaderDataID.enableScreenSpaceReflectionID);
                
                cmd.BeginSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
                
                GetSetClearRtTarget(renderingData, out int width, out int height);

                Draw(width, height);
                
                cmd.SetGlobalTexture(ssrTextureID, ssrRT);

                cmd.EndSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
            }

            private void Draw(int width, int height)
            {
                var cs = settings.computeShader;
                cmd.SetComputeVectorParam(cs, ssrParamsID, 
                    new Vector4(settings.maxStep, settings.stepSize, settings.thickness));
                cmd.SetComputeVectorParam(cs, texelSizeID,
                    new Vector4(width, height, 1 / (float)width, 1 / (float)height));
                
                cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.cameraColorCopyTextureID, renderingData.renderer.GetCameraColorBufferRT());
                cmd.SetComputeTextureParam(cs, kernel, renderingData.renderer.GetCameraDepthCopyRT(), renderingData.renderer.GetCameraDepthBufferRT());
                cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal], ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal]);
                cmd.SetComputeTextureParam(cs, kernel, ssrTextureID, ssrRT);
                
                int threadGroupX = Mathf.CeilToInt(width / 8.0f);
                int threadGroupY = Mathf.CeilToInt(height / 8.0f);
                cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
            }

            private void GetSetClearRtTarget(RenderingData renderingData,  out int width, out int height)
            {
                var descriptor = renderingData.cameraData.cameraColorDescriptor;
                descriptor.enableRandomWrite = true;
                descriptor.useMipMap = true;
                descriptor.autoGenerateMips = true;
                descriptor.mipCount = 7;
                width = descriptor.width;
                height =descriptor.height;
                
                cmd.SetRenderTarget(ssrRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            }

            public override void CleanUp()
            {
                
            }

            public void Dispose()
            {
                ssrRT?.Release();
            }
        }
    }
}