using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class ForwardTransparentPass : RoXamiRenderPass
    {
        const string bufferName = "RoXami Forward Transparent";
        public ForwardTransparentPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        CommandBuffer cmd;

        private RenderingData renderingData;
        private ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderData)
        {
            renderingData = renderData;
            context = scriptableRenderContext;

            cmd = renderingData.commandBuffer;
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                SetRenderTarget(cmd);
                ExecuteCommandBuffer(context, cmd);
                
                DrawTransparent();
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
        }

        void DrawTransparent()
        {
            SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera)
            {
                criteria = SortingCriteria.CommonTransparent
            };
            DrawingSettings drawingSettings = new DrawingSettings(ShaderDataID.unlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching =
                    renderingData.commonSettings.enableDynamicBatching,
                enableInstancing =
                    renderingData.commonSettings.enableGpuInstancing,
                // perObjectData =
                //     PerObjectData.ReflectionProbes | PerObjectData.LightProbe
            };
            drawingSettings.SetShaderPassName(1, ShaderDataID.toonLitShaderTagId);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
#if UNITY_EDITOR
            drawingSettings.SetShaderPassName(2, ShaderDataID.unityLitShaderTagId);
            drawingSettings.SetShaderPassName(3, ShaderDataID.unityUnlitShaderTagId);
#endif

            context.DrawRenderers(
                renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        }
    }
}