using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class ForwardTransparentPass : RoXamiRenderPass
    {
        public ForwardTransparentPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        static readonly string transparentBufferName = "RoXami Forward Transparent";

        static readonly CommandBuffer transparentCmd = new CommandBuffer()
        {
            name = transparentBufferName
        };

        private RenderingData renderingData;
        private ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderData)
        {
            renderingData = renderData;
            context = scriptableRenderContext;

            SetRenderTarget(transparentCmd);
            transparentCmd.BeginSample(transparentBufferName);
            ExecuteCommandBuffer(context, transparentCmd);

            DrawTransparent();

            transparentCmd.EndSample(transparentBufferName);
            ExecuteCommandBuffer(context, transparentCmd);
        }

        public override void CleanUp()
        {
        }

        private void SetRenderTarget(CommandBuffer cmd)
        {
            cmd.SetRenderTarget(
                ShaderDataID.cameraColorAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                ShaderDataID.cameraDepthAttachmentId,
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

            context.Submit();
        }
    }
}