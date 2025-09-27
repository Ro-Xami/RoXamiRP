using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class ScreenSpaceShadowsPass : RoXamiRenderPass
    {
        public ScreenSpaceShadowsPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        const string bufferName = "RoXami ScreenSpaceShadows";

        private static readonly CommandBuffer cmd = new()
        {
            name = bufferName,
        };

        private static readonly string[] directionalFilterKeywords =
        {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };

        static readonly int screenSpaceShadowsTextureID = Shader.PropertyToID("_ScreenSpaceShadowsTexture");
        static readonly int textureSizeID = Shader.PropertyToID("_TextureSize");
        private const string kernelName = "ScreenSpaceShadows";
        RenderingData renderingData;

        ComputeShader cs;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            renderingData = renderData;

            cs = renderingData.shaderAsset.screenSpaceShadowComputeShader;

            if (cs && renderingData.cameraData.additionalCameraData.enableScreenSpaceShadows &&
                renderingData.runtimeData.isCastShadows)
            {
                int width = renderingData.cameraData.width;
                int height = renderingData.cameraData.height;
                cmd.GetTemporaryRT(screenSpaceShadowsTextureID,
                    width, height, 0, FilterMode.Point, RenderTextureFormat.R16,
                    RenderTextureReadWrite.Linear, 1, true);
                cmd.SetRenderTarget(screenSpaceShadowsTextureID,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                cmd.BeginSample(bufferName);
                ExecuteCommandBuffer(context, cmd);

                ComputeScreenSpaceShadows(width, height);

                cmd.EndSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
            }
            else
            {
                cmd.GetTemporaryRT(screenSpaceShadowsTextureID,
                    1, 1, 0, FilterMode.Point, RenderTextureFormat.R8);
                ExecuteCommandBuffer(context, cmd);
            }
        }

        public override void CleanUp()
        {
            if (cs)
            {
                cmd.ReleaseTemporaryRT(screenSpaceShadowsTextureID);
            }
        }

        private void ComputeScreenSpaceShadows(int width, int height)
        {
            int kernel = cs.FindKernel(kernelName);

            cmd.SetComputeVectorParam(cs,
                textureSizeID, new Vector4(width, height, 1f / width, 1f / height));
            cmd.SetComputeTextureParam(cs, kernel,
                ShaderDataID.directionalShadowAtlasID, ShaderDataID.directionalShadowAtlasID);
            cmd.SetComputeTextureParam(cs, kernel,
                screenSpaceShadowsTextureID, screenSpaceShadowsTextureID);

            int threadGroupX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupY = Mathf.CeilToInt(height / 8.0f);
            cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
        }
    }
}