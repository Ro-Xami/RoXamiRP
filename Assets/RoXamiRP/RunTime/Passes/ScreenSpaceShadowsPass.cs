using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class ScreenSpaceShadowsPass : RoXamiRenderPass
    {
        public ScreenSpaceShadowsPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        const string bufferName = "RoXami ScreenSpaceShadows";
        private CommandBuffer cmd;

        private const string screenSpaceShadowsRtName = "_ScreenSpaceShadowsTexture";
        static readonly int screenSpaceShadowsRtID = Shader.PropertyToID(screenSpaceShadowsRtName);
        private RTHandle screenSpaceShadowsRT;

        private RenderTextureDescriptor screenSpaceShadowsRtDesc = new RenderTextureDescriptor(1, 1)
        {
            depthBufferBits = 0,
            colorFormat = RenderTextureFormat.RFloat,
            enableRandomWrite = true,
            msaaSamples = 1,
        };
        
        static readonly int textureSizeID = Shader.PropertyToID("_TextureSize");
        private const string kernelName = "ScreenSpaceShadows";
        RenderingData renderingData;

        ComputeShader cs;
        private int kernel = -1;
        private RTHandle directionalShadowAtlas;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            renderingData = renderData;
            cmd = renderData.commandBuffer;

            directionalShadowAtlas = renderingData.cameraData.directionalLightShadowAtlas;
            cs = renderingData.shaderAsset.screenSpaceShadowComputeShader;

            if (!cs) 
            {
                cmd.DisableShaderKeyword(ShaderDataID.screenSpaceShadowsKeyword);
                ExecuteCommandBuffer(context, cmd);
                
                return;
            }

            kernel = cs.FindKernel(kernelName);
            
            if (kernel < 0 || !renderingData.runtimeData.isCastShadows ||
                directionalShadowAtlas == null || !directionalShadowAtlas.rt)
            {
                cmd.DisableShaderKeyword(ShaderDataID.screenSpaceShadowsKeyword);
                ExecuteCommandBuffer(context, cmd);
                
                return;
            }

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.EnableShaderKeyword(ShaderDataID.screenSpaceShadowsKeyword);
                
                int width = screenSpaceShadowsRtDesc.width = renderingData.cameraData.width;
                int height = screenSpaceShadowsRtDesc.height = renderingData.cameraData.height;

                RoXamiRTHandlePool.GetRTHandleIfNeeded(
                    ref screenSpaceShadowsRT, screenSpaceShadowsRtDesc, FilterMode.Bilinear, screenSpaceShadowsRtName);
                cmd.SetRenderTarget(screenSpaceShadowsRT,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

                ComputeScreenSpaceShadows(width, height);
                
                cmd.SetGlobalTexture(screenSpaceShadowsRtID, screenSpaceShadowsRT);
            }
            ExecuteCommandBuffer(context, cmd);
        }

        public override void Dispose()
        {
            screenSpaceShadowsRT?.Release();
        }

        private void ComputeScreenSpaceShadows(int width, int height)
        {
            cmd.SetComputeVectorParam(cs,
                textureSizeID, new Vector4(width, height, 1f / width, 1f / height));
            cmd.SetComputeTextureParam(cs, kernel,
                ShaderDataID.cameraDepthCopyTextureID, renderingData.renderer.GetCameraDepthBufferRT());
            cmd.SetComputeTextureParam(cs, kernel,
                ShaderDataID.directionalShadowAtlasID, renderingData.cameraData.directionalLightShadowAtlas);
            cmd.SetComputeTextureParam(cs, kernel,
                screenSpaceShadowsRtID, screenSpaceShadowsRT);

            int threadGroupX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupY = Mathf.CeilToInt(height / 8.0f);
            cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
        }
    }
}