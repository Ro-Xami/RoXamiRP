using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiPost
{
    const string bufferName = "RoXamiPost";

    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };

    ScriptableRenderContext context;

    Camera camera;

    RoXamiRenderer renderer;

    public bool IsActive => renderer != null;

    public void Setup(
        ScriptableRenderContext context , Camera camera  , RoXamiRenderer renderer)
    {
        this.context = context;
        this.camera = camera;
        this.renderer = renderer;
    }

    public void Render(int sourceID)
    {
        cmd.Blit(sourceID, BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}