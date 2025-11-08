using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class RenderingDebugPass : RoXamiRenderPass
    {
        public RenderingDebugPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
            
        const string bufferName = "RoXamiRP RenderingDebug";
        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName,
        };
            
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
                
            cmd.SetRenderTarget(
                BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare
            );
            cmd.DrawProcedural(
                Matrix4x4.identity, renderingData.shaderAsset.renderingDebugMaterial, 0,
                MeshTopology.Triangles, 3
            );
                
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }
    }
}