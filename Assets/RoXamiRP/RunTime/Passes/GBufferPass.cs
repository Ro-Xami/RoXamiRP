using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace RoXamiRenderPipeline
{
    public class GBufferPass : RoXamiRenderPass
    {
        public GBufferPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        const string bufferName = "RoXami GBuffer";

        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        static readonly ShaderTagId toonGBufferShaderTagId = new ShaderTagId("ToonGBuffer");

        static readonly int[] gBufferNameIDs = new int[]
        {
            Shader.PropertyToID("_GBuffer0"),
            Shader.PropertyToID("_GBuffer1"),
            Shader.PropertyToID("_GBuffer2"),
            Shader.PropertyToID("_GBuffer3"),
        };

        private static readonly RenderTargetIdentifier[] gBufferTargets = new RenderTargetIdentifier[]
        {
            new RenderTargetIdentifier(gBufferNameIDs[0]),
            new RenderTargetIdentifier(gBufferNameIDs[1]),
            new RenderTargetIdentifier(gBufferNameIDs[2]),
            new RenderTargetIdentifier(gBufferNameIDs[3]),
        };

        RenderingData renderingData;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            renderingData = renderData;
            GetGBufferRT();

            ClearCmdRenderTarget();
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            SetDrawingSettings(out var drawingSettings, out var filteringSettings);
            context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
            context.Submit();

            CopyCameraDepth();

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
            foreach (var gBufferID in gBufferNameIDs)
            {
                cmd.ReleaseTemporaryRT(gBufferID);
            }
        }

        private void CopyCameraDepth()
        {
            cmd.CopyTexture(ShaderDataID.cameraDepthAttachmentId, ShaderDataID.cameraDepthCopyTextureID);
        }

        private void GetGBufferRT()
        {
            int width = renderingData.cameraData.width;
            int height = renderingData.cameraData.height;

            cmd.GetTemporaryRT(gBufferNameIDs[0], renderingData.cameraData.cameraColorDescriptor,
                renderingData.cameraData.cameraColorFilterMode); //Albedo
            cmd.GetTemporaryRT(gBufferNameIDs[1], width, height, 0, FilterMode.Point,
                RenderTextureFormat.ARGBFloat); //normal
            cmd.GetTemporaryRT(gBufferNameIDs[2], width, height, 0, FilterMode.Point,
                RenderTextureFormat.ARGB32); //Metallic/Roughness/Ao
            cmd.GetTemporaryRT(gBufferNameIDs[3], renderingData.cameraData.cameraColorDescriptor,
                FilterMode.Point); //Emission
        }

        void ClearCmdRenderTarget()
        {
            cmd.SetRenderTarget(gBufferTargets, ShaderDataID.cameraDepthAttachmentId);

            cmd.ClearRenderTarget(true, true, Color.clear);
        }

        void SetDrawingSettings(out DrawingSettings drawingSettings, out FilteringSettings filteringSettings)
        {
            SortingSettings sortingSettings = new SortingSettings(renderingData.cameraData.camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            drawingSettings = new DrawingSettings(toonGBufferShaderTagId, sortingSettings)
            {
                enableDynamicBatching = renderingData.rendererAsset.commonSettings.enableDynamicBatching,
                enableInstancing = renderingData.rendererAsset.commonSettings.enableGpuInstancing,
                perObjectData =
                    PerObjectData.ReflectionProbes |
                    PerObjectData.LightProbe
            };

            filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        }
    }
}