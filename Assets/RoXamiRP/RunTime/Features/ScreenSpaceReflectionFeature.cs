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
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingDeferredDiffuse;
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
#if UNITY_EDITOR
            if (!IsGameOrSceneCamera(renderingData.cameraData.camera)) return;
#endif

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
                
                m_ProfilingSampler = new ProfilingSampler(bufferName);
            }

            private CommandBuffer cmd;

            const string ssrRtName = "_ScreenSpaceReflectionTexture";
            private readonly int ssrTextureID = Shader.PropertyToID(ssrRtName);
            private RTHandle ssrRT;
            
            private readonly int ssrParamsID = Shader.PropertyToID("_ssrParams");
            private readonly int texelSizeID = Shader.PropertyToID("_texelSize");

            private RenderingData renderingData;

            public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
            {

            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                this.renderingData = renderingData;
                cmd = renderingData.commandBuffer;
                
                if (settings == null || !settings.computeShader || kernel < 0)
                {
                    cmd.DisableShaderKeyword(ShaderDataID.enableScreenSpaceReflectionID);
                    return;
                }

                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    cmd.EnableShaderKeyword(ShaderDataID.enableScreenSpaceReflectionID);
                
                    GetSetClearRtTarget(out int width, out int height);
                    Draw(width, height);
                    cmd.SetGlobalTexture(ssrTextureID, ssrRT);
                }
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
                cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.cameraDepthCopyTextureID, renderingData.renderer.GetCameraDepthBufferRT());
                cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal], renderingData.cameraData.GBufferRTs[(int)GBufferTye.Normal]);
                cmd.SetComputeTextureParam(cs, kernel, ssrTextureID, ssrRT);
                
                int threadGroupX = Mathf.CeilToInt(width / 8.0f);
                int threadGroupY = Mathf.CeilToInt(height / 8.0f);
                cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
            }

            private void GetSetClearRtTarget(out int width, out int height)
            {
                var descriptor = renderingData.cameraData.cameraColorDescriptor;
                descriptor.enableRandomWrite = true;
                descriptor.useMipMap = true;
                descriptor.autoGenerateMips = true;
                descriptor.mipCount = 7;
                width = descriptor.width;
                height =descriptor.height;

                RoXamiRTHandlePool.GetRTHandleIfNeeded(ref ssrRT, descriptor, FilterMode.Bilinear, ssrRtName);
                
                cmd.SetRenderTarget(ssrRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            }

            public override void Dispose()
            {
                ssrRT?.Release();
            }
        }
    }
}