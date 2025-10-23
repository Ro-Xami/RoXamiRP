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

        private static readonly RenderTargetIdentifier[] gBufferTargets = new RenderTargetIdentifier[]
        {
            new RenderTargetIdentifier(ShaderDataID.gBufferNameIDs[(int)GBufferTye.Albedo]),
            new RenderTargetIdentifier(ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal]),
            new RenderTargetIdentifier(ShaderDataID.gBufferNameIDs[(int)GBufferTye.MRA]),
            new RenderTargetIdentifier(ShaderDataID.gBufferNameIDs[(int)GBufferTye.Emission]),
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

            // RoXamiRPCopyTexture(cmd, 
            //     ShaderDataID.cameraDepthAttachmentId, 
            //     ShaderDataID.cameraDepthCopyTextureID);

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
            foreach (var gBufferID in ShaderDataID.gBufferNameIDs)
            {
                cmd.ReleaseTemporaryRT(gBufferID);
            }
        }

        private void GetGBufferRT()
        {
            int width = renderingData.cameraData.width;
            int height = renderingData.cameraData.height;

            cmd.GetTemporaryRT(ShaderDataID.gBufferNameIDs[(int)GBufferTye.Albedo], 
                renderingData.cameraData.cameraColorDescriptor, renderingData.cameraData.cameraColorFilterMode);
            
            cmd.GetTemporaryRT(ShaderDataID.gBufferNameIDs[(int)GBufferTye.Normal], 
                width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);

            cmd.GetTemporaryRT(ShaderDataID.gBufferNameIDs[(int)GBufferTye.MRA],
                width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            cmd.GetTemporaryRT(ShaderDataID.gBufferNameIDs[(int)GBufferTye.Emission],
                renderingData.cameraData.cameraColorDescriptor, FilterMode.Bilinear);
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

            drawingSettings = new DrawingSettings(ShaderDataID.toonGBufferShaderTagId, sortingSettings)
            {
                enableDynamicBatching = renderingData.commonSettings.enableDynamicBatching,
                enableInstancing = renderingData.commonSettings.enableGpuInstancing,
                // perObjectData =
                //     PerObjectData.ReflectionProbes |
                //     PerObjectData.LightProbe
            };

            filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        }
    }
}