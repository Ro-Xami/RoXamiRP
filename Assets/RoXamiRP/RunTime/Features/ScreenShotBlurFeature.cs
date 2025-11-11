using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public enum BlurMode
    {
        Gaussian,
        Box
    }
    
    [Serializable]
    public class BlurSettings
    {
        public int blurSize = 1;
        public int blurIterations = 3;
        public int rtDownScale = 2;
    }

    public class ScreenShotBlurFeature : RoXamiRenderFeature
    {
        [SerializeField]
        BlurMode blurMode = BlurMode.Gaussian;
        
        [SerializeField]
        BlurSettings blurSettings;
        
        private Material blurMaterial;

        private static BlurPass pass;
        
        public static void BeginBlur()
        {
            if (pass == null)
            {
                return;
            }
            
            RoXamiFeatureManager.Instance.SetActive(RoXamiFeatureStack.ScreenShotBlurUI, true);
        }

        public static void EndBlur()
        {
            pass?.Dispose();
        }
        
        public override void Create()
        {
            blurMaterial = CoreUtils.CreateEngineMaterial("RoXamiRP/Hidden/Blur");
            pass = new BlurPass(blurMode, blurSettings, blurMaterial);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!IsGameOrSceneCamera(renderingData.cameraData.camera)) return;
#endif
            
            if (RoXamiFeatureManager.Instance.IsActive(RoXamiFeatureStack.ScreenShotBlurUI))
            {
                renderer.EnqueuePass(pass);
                RoXamiFeatureManager.Instance.SetActive(RoXamiFeatureStack.ScreenShotBlurUI, false);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(blurMaterial);
            pass?.Dispose();
        }

        private class BlurPass : RoXamiRenderPass
        {
            private readonly BlurMode mode;
            private readonly BlurSettings settings;
            private readonly Material blurMaterial;
            public BlurPass(BlurMode mode, BlurSettings settings, Material blurMaterial)
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                this.mode = mode;
                this.settings = settings;
                this.blurMaterial = blurMaterial;
                
                m_ProfilingSampler = new ProfilingSampler(nameof(BlurPass));
            }

            private const string bufferName = "RoXamiRP ScreenShot Blur";
            private CommandBuffer cmd;

            private RenderTexture screenShotBlurRT;

            const string gaussianUpSampleName = "_GaussianUpSample";
            const string gaussianDownSampleName = "_GaussianDownSample";
            private RTHandle gaussianUpSampleRT;
            private RTHandle gaussianDownSampleRT;
            
            private static readonly int offsetID = Shader.PropertyToID("_Post_GaussianBlurOffset");
            private static readonly int screenShotBlurTextureID = Shader.PropertyToID("_ScreenShotBlurTexture");

            public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (settings == null)
                {
                    return;
                }
                
                cmd = renderingData.commandBuffer;
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    Blur(context, renderingData);
                }
                ExecuteCommandBuffer(context, cmd);
            }

            public override void Dispose()
            {
                gaussianUpSampleRT?.Release();
                gaussianDownSampleRT?.Release();
                screenShotBlurRT?.Release();
            }
            
            private void Blur(ScriptableRenderContext context, RenderingData renderingData)
            {
                var cameraData = renderingData.cameraData;
                int width = Mathf.Max(2, cameraData.cameraColorDescriptor.width / settings.rtDownScale);
                int height = Mathf.Max(2, cameraData.cameraColorDescriptor.height / settings.rtDownScale);

                var descriptor = cameraData.cameraColorDescriptor;
                descriptor.width = width;
                descriptor.height = height;

                RoXamiRTHandlePool.GetRTHandleIfNeeded(
                    ref gaussianUpSampleRT, descriptor, FilterMode.Bilinear, gaussianUpSampleName);
                RoXamiRTHandlePool.GetRTHandleIfNeeded(
                    ref gaussianDownSampleRT, descriptor, FilterMode.Bilinear, gaussianDownSampleName);

                screenShotBlurRT = RenderTexture.GetTemporary(descriptor);

                float offsetX = 1f / width * settings.blurSize;
                float offsetY = 1f / height * settings.blurSize;
                
                for (var i = 0; i < settings.blurIterations; i++)
                {
                    cmd.SetGlobalVector(offsetID, new Vector4(offsetX, 0f, 0f, 0f));
                    DrawDontCareDontCare
                    (
                        cmd, 
                        i == 0 ? renderingData.renderer.GetCameraColorBufferRT(): gaussianDownSampleRT, 
                        gaussianUpSampleRT, 
                        blurMaterial, 0
                    );
                    
                    cmd.SetGlobalVector(offsetID, new Vector4(0f, offsetY, 0f, 0f));
                    DrawDontCareDontCare
                    (
                        cmd, 
                        gaussianUpSampleRT, 
                        i == settings.blurIterations - 1? screenShotBlurRT: gaussianDownSampleRT,
                        blurMaterial, 0
                    );
                }
                Shader.SetGlobalTexture(screenShotBlurTextureID, screenShotBlurRT);
            }
            
        }
    }
}