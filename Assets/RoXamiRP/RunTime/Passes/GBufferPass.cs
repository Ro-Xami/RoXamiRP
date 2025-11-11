using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace RoXamiRP
{
    public class GBufferPass : RoXamiRenderPass
    {
        public GBufferPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        const string bufferName = "RoXami GBuffer";
        private CommandBuffer cmd;
        
        const int albedoIndex = (int)GBufferTye.Albedo;
        const int normalIndex = (int)GBufferTye.Normal;
        const int mraIndex = (int)GBufferTye.MRA;
        const int emissionIndex = (int)GBufferTye.Emission;

        private readonly RTHandle[] GBufferRTs = new RTHandle[ShaderDataID.gBufferNameIDs.Length];
        private readonly RenderTargetIdentifier[] GBufferTargets = new RenderTargetIdentifier[ShaderDataID.gBufferNameIDs.Length];

        RenderingData renderingData;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingDataInOut)
        {
            this.renderingData = renderingDataInOut;
            cmd = renderingData.commandBuffer;
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                GetGBufferRT(ref renderingDataInOut);
                ClearCmdRenderTarget();
                
                ExecuteCommandBuffer(context, cmd);

                SetDrawingSettings(out var drawingSettings, out var filteringSettings);
                context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
                
                ExecuteCommandBuffer(context, cmd);
                
                SetGBufferRT();
            }
            ExecuteCommandBuffer(context, cmd);
            
        }

        public override void Dispose()
        {
            foreach (var GBuffer in GBufferRTs)
            {
                GBuffer?.Release();
            }
        }

        private void GetGBufferRT(ref RenderingData renderingDataOutput)
        {
            var baseDescriptor = renderingData.cameraData.cameraColorDescriptor;
            var filterMode = renderingData.cameraData.cameraColorFilterMode;

            for (int GBufferIndex = 0; GBufferIndex < ShaderDataID.gBufferNameIDs.Length; GBufferIndex++)
            {
                var descriptor = baseDescriptor;
                switch ((GBufferTye)GBufferIndex)
                {
                    case GBufferTye.Albedo:
                        break;
                    
                    case GBufferTye.Normal:
                        descriptor.colorFormat = RenderTextureFormat.ARGBFloat;
                        break;
                    
                    case GBufferTye.MRA:
                        descriptor.colorFormat = RenderTextureFormat.ARGB32;
                        break;
                    
                    case GBufferTye.Emission:
                        break;
                }

                var need = RoXamiRTHandlePool.GetRTHandleIfNeeded(
                    ref GBufferRTs[GBufferIndex], descriptor, filterMode,
                    ShaderDataID.gBufferNames[GBufferIndex]);
                
                GBufferTargets[GBufferIndex] = GBufferRTs[GBufferIndex];
            }

            renderingDataOutput.cameraData.GBufferRTs = GBufferRTs;
        }

        void SetGBufferRT()
        {
            for (int GBufferIndex = 0; GBufferIndex < ShaderDataID.gBufferNameIDs.Length; GBufferIndex++)
            {
                cmd.SetGlobalTexture(ShaderDataID.gBufferNameIDs[GBufferIndex], GBufferRTs[GBufferIndex]);
            }
        }

        void ClearCmdRenderTarget()
        {
            cmd.SetRenderTarget(GBufferTargets, renderingData.renderer.GetCameraDepthBufferRT());

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