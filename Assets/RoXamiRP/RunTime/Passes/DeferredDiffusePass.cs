using System;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace RoXamiRP
{
    public class DeferredDiffusePass : RoXamiRenderPass
    {
        Material material;
        public DeferredDiffusePass(RenderPassEvent evt, Material material)
        {
            renderPassEvent = evt;
            this.material = material;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        private const string bufferName = "RoXami Deferred";
        private CommandBuffer cmd;

        private ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderingData)
        {
            context = scriptableRenderContext;
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
            if (!material)
            {
                material = renderingData.shaderAsset.deferredMaterial;
            }
            
            cmd.SetRenderTarget(
                renderingData.renderer.GetCameraColorBufferRT(),
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                renderingData.renderer.GetCameraDepthBufferRT(),
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(false, true, Color.clear);
            
            cmd.DrawProcedural(
                Matrix4x4.identity, material, 0,
                MeshTopology.Triangles, 3
            );
        }
    }
}