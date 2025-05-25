using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiPost
{
    public bool IsActive => renderer != null;
    
    const string bufferName = "RoXamiPost";
    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };
    ScriptableRenderContext context;
    Camera camera;
    RoXamiRenderer renderer;
    
    int postSourceId = Shader.PropertyToID("_PostSource");

    enum Pass
    {
        Copy,
    };

    public void Setup(
        ScriptableRenderContext context , Camera camera  , RoXamiRenderer renderer)
    {
        this.context = context;
        this.camera = camera;
        this.renderer = camera.cameraType <= CameraType.SceneView ? renderer : null;
    }

    public void Render(int sourceID)
    {
        Draw(sourceID, BuiltinRenderTextureType.CameraTarget, Pass.Copy);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    
    void Draw (
        RenderTargetIdentifier from, RenderTargetIdentifier to, Pass pass
    ) {
        cmd.SetGlobalTexture(postSourceId, from);
        cmd.SetRenderTarget(
            to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );
        cmd.DrawProcedural(
            Matrix4x4.identity, renderer.Material, (int)pass,
            MeshTopology.Triangles, 3
        );
    }
}