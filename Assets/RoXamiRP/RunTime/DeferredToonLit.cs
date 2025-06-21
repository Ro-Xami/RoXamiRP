using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class DeferredToonLit
{
    const string bufferName = "RoXami DeferredToonLit";

    private CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };

    public void SetUp()
    {
        
    }

    public void Render(ScriptableRenderContext context, RoXamiRenderer renderer)
    {
        cmd.BeginSample(bufferName);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        
        cmd.Blit(BuiltinRenderTextureType.CameraTarget,BuiltinRenderTextureType.CameraTarget, renderer.deferredMaterial);
        
        context.Submit();
        cmd.EndSample(bufferName);
        
        //?
        // cmd.DrawProcedural(
        //     Matrix4x4.identity, renderer.DeferredMaterial, 0,
        //     MeshTopology.Triangles, 3
        // );
        // cmd.SetRenderTarget(
        //     BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        // );
    }

    public void CleanUp()
    {
        
    }
}