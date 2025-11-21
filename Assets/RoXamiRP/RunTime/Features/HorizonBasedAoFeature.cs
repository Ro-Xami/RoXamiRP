using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class HorizonBasedAoFeature : RoXamiRenderFeature
    {
        [SerializeField]
        HBAoSettings settings;
        
        HBAoPass pass;

        [Serializable]
        private class HBAoSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingDeferredGI;
            public ComputeShader computeShader;
            
            [Range(1,4)] public int downsample = 1;
            [Min(0.0f)] public float intensity = 1.0f;
            [Range(0.0f, 1.0f)] public float radius = 0.5f;
            [Min(0.0f)] public float maxStepSize = 10.0f;
            [Range(0.0f, 1.0f)] public float angleBias = 0.1f;

            [Space(10)]
            [Range(1, 4)] public int blurDownSample = 1;
            [Range(1, 4)] public int blurIterations = 1;
            [Min(0.0f)] public float blurSize = 1f;
        }
        
        public override void Create()
        {
            if (settings == null || !settings.computeShader) return;

            pass = new HBAoPass(settings);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
            if (pass != null)
            {
                renderer.EnqueuePass(pass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            pass?.Dispose();
        }
        
        private class HBAoPass : RoXamiRenderPass
        {
            public HBAoPass(HBAoSettings settings)
            {
                this.settings = settings;
                this.renderPassEvent = settings.renderPassEvent;
                cs = settings.computeShader;

                if (!cs) return;
                kernel = cs.FindKernel(kernelName);

                m_ProfilingSampler = new ProfilingSampler(bufferName);
            }

            const string kernelName = "HBAO";
            private readonly int kernel;
            private readonly ComputeShader cs;

            const string hbaoRtName = "_HBAoTexture";
            static readonly int hbaoRtID = Shader.PropertyToID(hbaoRtName);
            private RTHandle hbaoRT;
            
            private static readonly int texelSizeID = Shader.PropertyToID("_texelSize");
            private static readonly int hbaoParamsID = Shader.PropertyToID("_hbaoParams");
            private static readonly int hbaoStepSizeID = Shader.PropertyToID("_stepSize");

            private RenderTextureDescriptor hbaoDescriptor = new RenderTextureDescriptor(1, 1)
            {
                colorFormat = RenderTextureFormat.RFloat,
                enableRandomWrite = true,
                depthBufferBits = 0,
                msaaSamples = 1,
            };
            
            private Material m_Material;
            private Material blurMaterial
            {
                get
                {
                    if (!m_Material)
                    {
                        m_Material = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/Blur");
                    }
                    return m_Material;
                }
            }
            
            const string blurHBAoRtNameH = "_HBAoBlurTextureH";
            const string blurHBAoRtNameV = "_HBAoBlurTextureV";
            private RTHandle blurHorizonHBAoRT;
            private RTHandle blurVerticalHBAoRT;
            private RenderTextureDescriptor blurDescriptor = new RenderTextureDescriptor(1, 1)
            {
                colorFormat = RenderTextureFormat.RFloat,
                depthBufferBits = 0,
                msaaSamples = 1,
            };
            
            private static readonly int blurOffsetID = Shader.PropertyToID("_Post_GaussianBlurOffset");

            const string bufferName = "RoXamiRP HBAO";
            private CommandBuffer cmd;
            private readonly HBAoSettings settings;

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!cs || kernel < 0) return;
                
                cmd = renderingData.commandBuffer;
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    //GetSetRTHandle(renderingData, out int width, out int height);
                    ComputeHBAO(renderingData);

                    Blur(renderingData);

                    cmd.EnableShaderKeyword(ShaderDataID.horizonBasedAoKeyword);
                }
                ExecuteCommandBuffer(context, cmd);
                
            }

            public override void Dispose()
            {
                hbaoRT?.Release();
                blurHorizonHBAoRT?.Release();
                blurVerticalHBAoRT?.Release();
                CoreUtils.Destroy(m_Material);
            }
            
            private void GetSetRTHandle(RenderingData renderingData, out int width, out int height)
            {
                hbaoDescriptor.width = width = Mathf.Max(2, renderingData.cameraData.width / settings.downsample);
                hbaoDescriptor.height = height = Mathf.Max(2, renderingData.cameraData.height / settings.downsample);
                RoXamiRTHandlePool.GetRTHandleIfNeeded(ref hbaoRT, hbaoDescriptor, FilterMode.Bilinear, bufferName);
                cmd.SetRenderTarget(hbaoRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            }

            void ComputeHBAO(RenderingData renderingData)
            {
                int width = hbaoDescriptor.width = Mathf.Max(2, renderingData.cameraData.width / settings.downsample);
                int height = hbaoDescriptor.height = Mathf.Max(2, renderingData.cameraData.height / settings.downsample);
                
                RoXamiRTHandlePool.GetRTHandleIfNeeded(ref hbaoRT, hbaoDescriptor, FilterMode.Bilinear, bufferName);
                cmd.SetRenderTarget(hbaoRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                
                cmd.SetComputeVectorParam(cs,
                    texelSizeID, new Vector4(width, height, 1f / width, 1f / height));
                var tanHalfFovY = Mathf.Tan(renderingData.cameraData.camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                cmd.SetComputeFloatParam(cs, hbaoStepSizeID, 
                    renderingData.cameraData.camera.pixelHeight * settings.radius * 1.5f / tanHalfFovY / 2.0f);
                cmd.SetComputeVectorParam(cs, hbaoParamsID, 
                    new Vector4(settings.intensity, settings.radius, settings.maxStepSize, settings.angleBias));
                cmd.SetComputeTextureParam(cs, kernel,
                    ShaderDataID.cameraDepthCopyTextureID, renderingData.renderer.GetCameraDepthBufferRT());
                cmd.SetComputeTextureParam(cs, kernel,
                    ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal], renderingData.cameraData.GBufferRTs[(int)GBufferTye.Normal]);
                cmd.SetComputeTextureParam(cs, kernel,
                    hbaoRtID, hbaoRT);

                int threadGroupX = Mathf.CeilToInt(width / 8.0f);
                int threadGroupY = Mathf.CeilToInt(height / 8.0f);
                cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
                
                cmd.SetGlobalTexture(hbaoRtID, hbaoRT);
            }

            private void Blur(RenderingData renderingData)
            {
                if (settings.blurIterations <= 0 || settings.blurSize <= 0) return;
                
                var cameraData = renderingData.cameraData;
                blurDescriptor.width = Mathf.Max(2, cameraData.cameraColorDescriptor.width / settings.blurDownSample);
                blurDescriptor.height = Mathf.Max(2, cameraData.cameraColorDescriptor.height / settings.blurDownSample);

                RoXamiRTHandlePool.GetRTHandleIfNeeded(
                    ref blurHorizonHBAoRT, blurDescriptor, FilterMode.Bilinear, blurHBAoRtNameH);
                RoXamiRTHandlePool.GetRTHandleIfNeeded(
                    ref blurVerticalHBAoRT, blurDescriptor, FilterMode.Bilinear, blurHBAoRtNameV);

                float offsetX = 1f / blurDescriptor.width * settings.blurSize;
                float offsetY = 1f / blurDescriptor.height * settings.blurSize;
                
                for (var i = 0; i < settings.blurIterations; i++)
                {
                    cmd.SetGlobalVector(blurOffsetID, new Vector4(offsetX, 0f, 0f, 0f));
                    DrawDontCareDontCare
                    (
                        cmd, 
                        i == 0 ? hbaoRT: blurVerticalHBAoRT, 
                        blurHorizonHBAoRT, 
                        blurMaterial, 0
                    );
                    
                    cmd.SetGlobalVector(blurOffsetID, new Vector4(0f, offsetY, 0f, 0f));
                    DrawDontCareDontCare
                    (
                        cmd, 
                        blurHorizonHBAoRT, 
                        blurVerticalHBAoRT,
                        blurMaterial, 0
                    );
                }
                cmd.SetGlobalTexture(hbaoRtID, blurVerticalHBAoRT);
            }
            
        }
    }
}
