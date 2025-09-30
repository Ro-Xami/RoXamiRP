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
        
        public static void BeganBlur()
        {
            
        }
        
        public override void Create()
        {
            blurMaterial = CoreUtils.CreateEngineMaterial("RoXami RP/Hidden/Blur");
            pass = new BlurPass(blurMode, blurSettings, blurMaterial);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pass);
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

            private readonly int gaussianUpSample = Shader.PropertyToID("_GaussianUpSample");
            private readonly int gaussianDownSample = Shader.PropertyToID("_GaussianDownSample");
            private readonly int offsetID = Shader.PropertyToID("_Post_GaussianBlurOffset");

            public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
            {
                
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cameraData = renderingData.cameraData;

                var descriptor = cameraData.cameraColorDescriptor;
                descriptor.width = Mathf.Max(2, (int) cameraData.cameraColorDescriptor.width / settings.rtDownScale);
                descriptor.height = Mathf.Max(2, (int) cameraData.cameraColorDescriptor.height / settings.rtDownScale);
                
                cmd.GetTemporaryRT(gaussianUpSample, descriptor, FilterMode.Bilinear);
                cmd.GetTemporaryRT(gaussianDownSample, descriptor, FilterMode.Bilinear);

                // for (var i = 0; i < settings.blurIterations; i++)
                // {
                //     cmd.SetGlobalVector(offsetID, );
                //     DrawDontCareDontCare(
                //         cmd, i == 0 ? ShaderDataID.cameraColorAttachmentId: gaussianDownSample, 
                //         gaussianUpSample, blurMaterial, 0);
                //     cmd.SetGlobalVector(offsetID, );
                //     DrawDontCareDontCare(
                //         cmd, gaussianUpSample, 
                //         gaussianDownSample, blurMaterial, 0);
                // }
            }

            public override void CleanUp()
            {
                cmd.ReleaseTemporaryRT(gaussianUpSample);
                cmd.ReleaseTemporaryRT(gaussianDownSample);
            }
        }
    }
}