using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class DeferredPass
{
    const string bufferName = "RoXami Deferred";
    private CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };
    
    ScriptableRenderContext context;

    public void SetUp(RenderingData renderingData)
    {
        context = renderingData.context;
        
        SetRenderTarget(renderingData);
        cmd.BeginSample(bufferName);
        ExecuteBuffer();

        Draw(renderingData);

        cmd.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void SetRenderTarget(RenderingData renderingData)
    {
        cmd.SetRenderTarget(
            renderingData.cameraColorAttachmentId,
            RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
            renderingData.cameraDepthAttachmentId,
            RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
    }

    // void Draw(RenderingData renderingData)
    // {
    //     Camera camera = renderingData.camera;
    //     cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
    //     cmd.SetViewport(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight));
    //     cmd.DrawMesh(FullscreenMeshUtils.fullscreenQuad, Matrix4x4.identity, renderingData.renderer.deferredMaterial);
    //     cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix); 
    // }

    void Draw(RenderingData renderingData)
    {
         cmd.DrawProcedural(
             Matrix4x4.identity, renderingData.renderer.deferredMaterial, 0,
             MeshTopology.Triangles, 3
         );
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        
    }
}