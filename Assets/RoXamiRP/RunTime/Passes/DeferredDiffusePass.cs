using System;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace RoXamiRenderPipeline
{
    public class DeferredDiffusePass : RoXamiRenderPass
    {
        Material material;
        public DeferredDiffusePass(RenderPassEvent evt, Material material)
        {
            renderPassEvent = evt;
            this.material = material;
        }

        const string bufferName = "RoXami Deferred";

        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        private ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderingData)
        {
            context = scriptableRenderContext;

            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            Draw(renderingData);

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {

        }

        void Draw(RenderingData renderingData)
        {
            if (!material)
            {
                material = renderingData.shaderAsset.deferredMaterial;
            }
            
            cmd.SetRenderTarget(
                ShaderDataID.cameraColorAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                ShaderDataID.cameraDepthAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(false, true, Color.clear);
            
            cmd.DrawProcedural(
                Matrix4x4.identity, material, 0,
                MeshTopology.Triangles, 3
            );
        }
    }
}