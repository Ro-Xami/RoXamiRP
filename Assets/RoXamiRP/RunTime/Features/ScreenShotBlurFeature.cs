using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
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
            pass?.ReleaseScreenShotBlurRT();
        }
        
        public override void Create()
        {
            blurMaterial = CoreUtils.CreateEngineMaterial("RoXamiRP/Hidden/Blur");
            pass = new BlurPass(blurMode, blurSettings, blurMaterial);
        }

        public override void AddRenderPasses(RoXamiRenderLoop renderLoop, ref RenderingData renderingData)
        {
            if (RoXamiFeatureManager.Instance.IsActive(RoXamiFeatureStack.ScreenShotBlurUI))
            {
                renderLoop.EnqueuePass(pass);
                RoXamiFeatureManager.Instance.SetActive(RoXamiFeatureStack.ScreenShotBlurUI, false);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(blurMaterial);
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
            }

            private const string bufferName = "RoXamiRP ScreenShot Blur";
            private CommandBuffer cmd = new CommandBuffer()
            {
                name = bufferName
            };

            private RenderTexture screenShotBlurRT;

            private readonly int gaussianUpSample = Shader.PropertyToID("_GaussianUpSample");
            private readonly int gaussianDownSample = Shader.PropertyToID("_GaussianDownSample");
            private readonly int offsetID = Shader.PropertyToID("_Post_GaussianBlurOffset");
            
            private readonly int screenShotBlurTextureID = Shader.PropertyToID("_ScreenShotBlurTexture");

            public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                Blur(context, renderingData);
            }

            public override void CleanUp()
            {
                cmd.ReleaseTemporaryRT(gaussianUpSample);
                cmd.ReleaseTemporaryRT(gaussianDownSample);
            }

            public void ReleaseScreenShotBlurRT()
            {
                screenShotBlurRT?.Release();
            }
            
            private void Blur(ScriptableRenderContext context, RenderingData renderingData)
            {
                if (settings == null)
                {
                    return;
                }
                
                var cameraData = renderingData.cameraData;
                int width = Mathf.Max(2, cameraData.cameraColorDescriptor.width / settings.rtDownScale);
                int height = Mathf.Max(2, cameraData.cameraColorDescriptor.height / settings.rtDownScale);

                var descriptor = cameraData.cameraColorDescriptor;
                descriptor.width = width;
                descriptor.height = height;
                
                cmd.GetTemporaryRT(gaussianUpSample, descriptor, FilterMode.Bilinear);
                cmd.GetTemporaryRT(gaussianDownSample, descriptor, FilterMode.Bilinear);
                screenShotBlurRT = RenderTexture.GetTemporary(descriptor);

                float offsetX = 1f / width * settings.blurSize;
                float offsetY = 1f / height * settings.blurSize;

                cmd.BeginSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
                
                for (var i = 0; i < settings.blurIterations; i++)
                {
                    cmd.SetGlobalVector(offsetID, new Vector4(offsetX, 0f, 0f, 0f));
                    DrawDontCareDontCare
                    (
                        cmd, 
                        i == 0 ? ShaderDataID.cameraColorAttachmentId: gaussianDownSample, 
                        gaussianUpSample, 
                        blurMaterial, 0
                    );
                    
                    cmd.SetGlobalVector(offsetID, new Vector4(0f, offsetY, 0f, 0f));
                    DrawDontCareDontCare
                    (
                        cmd, 
                        gaussianUpSample, 
                        i == settings.blurIterations - 1? screenShotBlurRT: gaussianDownSample,
                        blurMaterial, 0
                    );
                }
                
                Shader.SetGlobalTexture(screenShotBlurTextureID, screenShotBlurRT);
                
                cmd.EndSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
            }
        }
    }
}