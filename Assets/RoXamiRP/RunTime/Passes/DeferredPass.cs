using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class DeferredPass : RoXamiRenderPass
{
    public DeferredPass(RenderPassEvent evt)
    {
        renderPassEvent = evt;
    }
    const string bufferName = "RoXami Deferred";
    private CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        context = renderingData.context;

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
        
         cmd.DrawProcedural(
             Matrix4x4.identity, renderingData.RendererAsset.deferredMaterial, 0,
             MeshTopology.Triangles, 3
         );
    }

    
}