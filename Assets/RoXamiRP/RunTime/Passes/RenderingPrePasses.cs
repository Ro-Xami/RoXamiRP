using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
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

            if (renderingData.runtimeData.enablePostProcessing)
            {
                var camera = renderingData.cameraData.camera;
                VolumeManager.instance.Update(camera.transform, camera.cullingMask);
            }
        }
    }
}