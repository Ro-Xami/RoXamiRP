using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class DrawSkyboxPass : RoXamiRenderPass
    {
        public DrawSkyboxPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        static readonly string bufferName = "Draw Skybox";

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

            //Opaque==========================================================================
            SetRenderTarget(cmd);
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            DrawSkybox();

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
        }

        private void SetRenderTarget(CommandBuffer cmd)
        {
            cmd.SetRenderTarget(
                renderingData.cameraData.cameraColorAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                renderingData.cameraData.cameraDepthAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        }

        void DrawSkybox()
        {
            context.DrawSkybox(renderingData.cameraData.camera);
            CopyCameraColor(cmd);
            context.Submit();
        }

        void CopyCameraColor(CommandBuffer cmd)
        {
            cmd.CopyTexture(renderingData.cameraData.cameraColorAttachmentId, ShaderDataID.cameraColorCopyTextureID);
            cmd.CopyTexture(renderingData.cameraData.cameraDepthAttachmentId, ShaderDataID.cameraDepthCopyTextureID);
        }
    }
}