using System;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace RoXamiRenderPipeline
{
    public class DeferredGiPass : RoXamiRenderPass
    {
        public DeferredGiPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        const string bufferName = "RoXami Deferred GI";

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
            cmd.SetRenderTarget(
                ShaderDataID.cameraColorAttachmentId,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                ShaderDataID.cameraDepthAttachmentId, 
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            //cmd.ClearRenderTarget(false, false, Color.clear);
            
            cmd.DrawProcedural(
                Matrix4x4.identity, renderingData.shaderAsset.deferredMaterial, 1,
                MeshTopology.Triangles, 3
            );
        }
    }
}