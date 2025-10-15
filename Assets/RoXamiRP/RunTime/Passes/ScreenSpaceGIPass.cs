using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class ScreenSpaceGIPass : RoXamiRenderPass
    {
        public ScreenSpaceGIPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        private const string bufferName = "RoXami ScreenSpaceGIPass";
        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = "ScreenSpaceGIPass",
        };

        private readonly string kernelName = "ScreenSpaceGI";
        private ComputeShader cs;
        private RenderingData renderingData;
        
        private static readonly int 
            screenSpaceGiTextureID = Shader.PropertyToID("_ScreenSpaceGiTexture"),
            screenSpaceGiTextureSizeID = Shader.PropertyToID("_ScreenSpaceGiTexture_TexelSize");

        public override void SetUp(CommandBuffer commandBuffer, ref RenderingData renderData)
        {
            
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            this.renderingData = renderData;
            
            cs = renderingData.shaderAsset.screenSpaceGIComputeShader;
            if (!cs)
            {
                return;
            }
   
            var cameraData = renderingData.cameraData;
            int width = cameraData.width;
            int height = cameraData.height;
            cmd.GetTemporaryRT(screenSpaceGiTextureID,
                width, height, 0, FilterMode.Point, 
                RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear, 0, true);

            cmd.SetRenderTarget(screenSpaceGiTextureID,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            ComputeScreenSpaceShadows(width, height);

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

        }

        public override void CleanUp()
        {
            if (cs)
            {
                cmd.ReleaseTemporaryRT(screenSpaceGiTextureID);
            }
        }
        
        private void ComputeScreenSpaceShadows(int width, int height)
        {
            int kernel = cs.FindKernel(kernelName);

            cmd.SetComputeVectorParam(cs,
                screenSpaceGiTextureSizeID, 
                new Vector4(width, height, 1f / width, 1f / height));
            cmd.SetComputeTextureParam(cs, kernel,
                ShaderDataID.directionalShadowAtlasID, ShaderDataID.directionalShadowAtlasID);
            cmd.SetComputeTextureParam(cs, kernel,
                screenSpaceGiTextureID, screenSpaceGiTextureID);

            int threadGroupX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupY = Mathf.CeilToInt(height / 8.0f);
            cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
        }
    }
}