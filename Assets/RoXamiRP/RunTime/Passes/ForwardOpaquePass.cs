using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class ForwardOpaquePass : RoXamiRenderPass
    {
        public ForwardOpaquePass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        const string bufferName = "RoXami Forward Opaque";

        private RenderingData renderingData;
        private ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderData)
        {
            renderingData = renderData;
            context = scriptableRenderContext;

            CommandBuffer cmd = renderingData.commandBuffer;
            
            SetRenderTarget(cmd);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                ExecuteCommandBuffer(context, cmd);

                DrawOpaque();
            }
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp(CommandBuffer commandBuffer)
        {
        }

        private void SetRenderTarget(CommandBuffer cmd)
        {
            cmd.SetRenderTarget(
                renderingData.renderer.GetCameraColorBufferRT(),
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                renderingData.renderer.GetCameraDepthBufferRT(),
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);

            if (renderingData.rendererSettings.rendererType == RendererType.Forward)
            {
                cmd.ClearRenderTarget(
                    true,
                    renderingData.cameraData.additionalCameraData.cameraRenderType == CameraRenderType.Base,
                    Color.clear);
            }
        }

        void DrawOpaque()
        {
            SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            DrawingSettings drawingSettings = new DrawingSettings(ShaderDataID.unlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = renderingData.commonSettings.enableDynamicBatching,
                enableInstancing = renderingData.commonSettings.enableGpuInstancing,
                // perObjectData =
                //     PerObjectData.ReflectionProbes |
                //     PerObjectData.LightProbe
            };
            drawingSettings.SetShaderPassName(1, ShaderDataID.toonLitShaderTagId);
#if UNITY_EDITOR
            drawingSettings.SetShaderPassName(2, ShaderDataID.unityLitShaderTagId);
            drawingSettings.SetShaderPassName(3, ShaderDataID.unityUnlitShaderTagId);
#endif

            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        }
    }
}