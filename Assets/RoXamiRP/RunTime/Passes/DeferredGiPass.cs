using System;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace RoXamiRP
{
    public class DeferredGiPass : RoXamiRenderPass
    {
        public DeferredGiPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        const string bufferName = "RoXami Deferred GI";

        private CommandBuffer cmd;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd = renderingData.commandBuffer;

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                Draw(renderingData);
            }
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp(CommandBuffer commandBuffer)
        {

        }

        void Draw(RenderingData renderingData)
        {
            cmd.SetRenderTarget(
                renderingData.renderer.GetCameraColorBufferRT(),
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                 renderingData.renderer.GetCameraDepthBufferRT(), 
                 RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            //cmd.ClearRenderTarget(false, false, Color.clear);
            
            cmd.DrawProcedural(
                Matrix4x4.identity, renderingData.shaderAsset.deferredMaterial, 1,
                MeshTopology.Triangles, 3
            );
        }
    }
}