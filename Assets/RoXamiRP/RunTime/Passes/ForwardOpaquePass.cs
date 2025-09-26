using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class ForwardOpaquePass : RoXamiRenderPass
    {
        public ForwardOpaquePass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        static readonly string bufferName = "RoXami Forward Opaque";

        static readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        private RenderingData renderingData;
        private ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderData)
        {
            renderingData = renderData;
            context = scriptableRenderContext;

            SetRenderTarget();
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            DrawOpaque();

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
        }

        private void SetRenderTarget()
        {
            cmd.SetRenderTarget(
                ShaderDataID.cameraColorAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                ShaderDataID.cameraDepthAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);

            if (!renderingData.rendererAsset.rendererSettings.enableDeferredRendering)
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
                perObjectData =
                    PerObjectData.ReflectionProbes |
                    PerObjectData.LightProbe
            };
            drawingSettings.SetShaderPassName(1, ShaderDataID.toonLitShaderTagId);

            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);

            CopyCameraColor(cmd);
            
            context.Submit();
        }
        
        void CopyCameraColor(CommandBuffer cmd)
        {
            cmd.CopyTexture(ShaderDataID.cameraColorAttachmentId, ShaderDataID.cameraColorCopyTextureID);
            cmd.CopyTexture(ShaderDataID.cameraDepthAttachmentId, ShaderDataID.cameraDepthCopyTextureID);
        }
    }
}