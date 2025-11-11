using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class DrawSkyboxPass : RoXamiRenderPass
    {
        const string bufferName = "Draw Skybox";
        public DrawSkyboxPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        private CommandBuffer cmd;

        private RenderingData renderingData;
        private ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderData)
        {
            renderingData = renderData;
            context = scriptableRenderContext;

            //Opaque==========================================================================
            cmd = renderData.commandBuffer;
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                SetRenderTarget(cmd);
                ExecuteCommandBuffer(context, cmd);
                
                DrawSkybox();
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

        void DrawSkybox()
        {
            context.DrawSkybox(renderingData.cameraData.camera);
        }
    }
}