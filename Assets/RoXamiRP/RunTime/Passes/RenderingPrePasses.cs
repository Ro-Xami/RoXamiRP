using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class RenderingPrePasses : RoXamiRenderPass
    {
        public RenderingPrePasses(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        public override void SetUp(CommandBuffer buffer, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            context.SetupCameraProperties(renderingData.cameraData.camera);
        }
    }
}