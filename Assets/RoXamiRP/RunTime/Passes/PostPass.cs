using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class PostPass : RoXamiRenderPass
    {
        const string postBufferName = "RoXami Post";
        ProfilingSampler bloomProfilingSampler = new ProfilingSampler("RoXami Bloom");
        public PostPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(postBufferName);
        }

        private CommandBuffer cmd;
        ScriptableRenderContext context;
        
        const string bloomFilterName = "_BloomFilter";
        private readonly RTHandle[] bloomDownSampleIDs = new RTHandle[8];
        private readonly RTHandle[] bloomUpSampleIDs = new RTHandle[8];

        RenderingData renderingData;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            renderingData = renderData;
            this.context = context;
            cmd = renderingData.commandBuffer;

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                using (new ProfilingScope(cmd, bloomProfilingSampler))
                {
                    Bloom bloomSettings = RoXamiVolume.Instance.GetVolumeComponent<Bloom>();
                    if (bloomSettings && bloomSettings.isActive)
                    {
                        SetupBloom(bloomSettings);
                    }
                }
                
                //Combine Post Process
                Draw
                (
                    cmd, 
                    renderingData.renderer.GetCameraColorBufferRT(),
                    //==================================================
                    //Draw to
                    renderingData.runtimeData.isCameraStackFinally && 
                    !renderingData.runtimeData.isAntialiasing?
                        
                        BuiltinRenderTextureType.CameraTarget:
                        renderingData.renderer.GetSwitchCameraColorBufferRT(),
                    //==================================================
                    PostShaderPass.combine
                );
            }

            ExecuteCommandBuffer(context, cmd);
        }

        public override void Dispose()
        {
            foreach (var bloomDown in bloomDownSampleIDs)
            {
                bloomDown?.Release();
            }

            foreach (var bloomUp in bloomUpSampleIDs)
            {
                bloomUp?.Release();
            }
        }

        #region Bloom
        void SetupBloom(Bloom bloomSettings)
        {
            int sampleCount = 
                bloomSettings.maxSampleCount >= bloomDownSampleIDs.Length ? 
                bloomDownSampleIDs.Length : bloomSettings.maxSampleCount;
        
            if (sampleCount <= 0)
            {
                return;
            }

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraColorDescriptor;
            FilterMode filter = FilterMode.Bilinear;
            
            //Filter
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref bloomUpSampleIDs[0], descriptor, filter, bloomFilterName);
            Draw(cmd, renderingData.renderer.GetCameraColorBufferRT(), bloomUpSampleIDs[0], PostShaderPass.filter);
            ExecuteCommandBuffer(context, cmd);
            
            var fromID = bloomUpSampleIDs[0];
            for (int i = 0; i < sampleCount; i++)
            {
                if (descriptor.width <= 0 || descriptor.height <= 0)
                {
                    sampleCount = i;
                    break;
                }

                RoXamiRTHandlePool.GetRTHandleIfNeeded(ref bloomDownSampleIDs[i], descriptor, filter, "_BloomDownSample" + i);
                RoXamiRTHandlePool.GetRTHandleIfNeeded(ref bloomUpSampleIDs[i], descriptor, filter, "_BloomUpSample" + i);

                Draw(cmd, fromID, bloomDownSampleIDs[i], PostShaderPass.blurH);
                Draw(cmd, bloomDownSampleIDs[i], bloomUpSampleIDs[i], PostShaderPass.blurV);
                
                ExecuteCommandBuffer(context, cmd);
        
                fromID = bloomUpSampleIDs[i];
                //lower the RT's size to the half size, and set Bilinear filterMode, to ues the unity's mipmap
                //a single mipmap0 1x1 pixel is the mipmap1 2x2 pixel's average color
                //example ,when size/2 and blur size is 3x3, same as the blur size is 6x6
                descriptor.width /= 2;
                descriptor.height /= 2;
            }
        
            
        
            //UpSample
            for (int i = sampleCount - 1; i > 0; i--)
            {
                cmd.SetGlobalTexture(ShaderDataID.TempRtSource1ID, bloomUpSampleIDs[i - 1]);
                Draw(cmd, bloomUpSampleIDs[i], bloomUpSampleIDs[i - 1], PostShaderPass.upSample);
            }
            cmd.SetGlobalTexture(ShaderDataID.TempRtSource1ID, bloomUpSampleIDs[0]);
        }
        #endregion

        void Draw(CommandBuffer cmd,
            RenderTargetIdentifier from, RenderTargetIdentifier to, PostShaderPass pass
        )
        {
            cmd.SetGlobalTexture(ShaderDataID.TempRtSource0ID, from);
            cmd.SetRenderTarget(
                to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
            cmd.DrawProcedural(
                Matrix4x4.identity, renderingData.shaderAsset.postMaterial, (int)pass,
                MeshTopology.Triangles, 3
            );
        }
    }
}