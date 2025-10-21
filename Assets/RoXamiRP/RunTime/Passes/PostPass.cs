using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class PostPass : RoXamiRenderPass
    {
        public PostPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        const string postBufferName = "RoXami Post";
        readonly CommandBuffer postCmd = new CommandBuffer
        {
            name = postBufferName,
        };

        const string bloomName = "RoXami Bloom";
        readonly CommandBuffer bloomCmd = new CommandBuffer
        {
            name = bloomName,
        };
        
        private static readonly int bloomFilterID = Shader.PropertyToID("_BloomFilter");
        private int[] bloomDownSampleIDs;
        private int[] bloomUpSampleIDs;

        RenderingData renderingData;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderData)
        {
            renderingData = renderData;

            postCmd.BeginSample(postBufferName);
            ExecuteCommandBuffer(context, postCmd);

            Bloom bloomSettings = RoXamiVolume.Instance.GetVolumeComponent<Bloom>();
            if (bloomSettings && bloomSettings.isActive)
            {
                bloomCmd.BeginSample(bloomName);
                ExecuteCommandBuffer(context, bloomCmd);
            
                SetupBloom(bloomSettings);
            
                bloomCmd.EndSample(bloomName);
                ExecuteCommandBuffer(context, bloomCmd);
            }
            
            Draw
            (
                postCmd, 
                ShaderDataID.cameraColorAttachmentId,
                //==================================================
                //Draw to
                renderingData.runtimeData.isFinalBlit && 
                !renderingData.cameraData.additionalCameraData.enableAntialiasing && 
                renderingData.antialiasingSettings.antialiasingMode != AntialiasingMode.None?
                    BuiltinRenderTextureType.CameraTarget:
                    ShaderDataID.cameraColorAttachmentId = ShaderDataID.cameraColorAttachmentId == ShaderDataID.cameraColorAttachmentAId?
                        ShaderDataID.cameraColorAttachmentBId : ShaderDataID.cameraColorAttachmentAId,
                //==================================================
                PostShaderPass.combine
            );

            postCmd.EndSample(postBufferName);
            ExecuteCommandBuffer(context, postCmd);
        }
        
        public override void CleanUp()
        {
            var bloomSettings = RoXamiVolume.Instance.GetVolumeComponent<Bloom>();
            if (bloomSettings && bloomSettings.isActive)
            {
                bloomCmd.ReleaseTemporaryRT(bloomFilterID);

                foreach (var bloomDownSampleID in bloomDownSampleIDs)
                {
                    bloomCmd.ReleaseTemporaryRT(bloomDownSampleID);
                }
                foreach (var bloomUpSampleID in bloomUpSampleIDs)
                {
                    bloomCmd.ReleaseTemporaryRT(bloomUpSampleID);
                }
                
                // for (int i = 0; i < bloomSettings.maxSampleCount; i++)
                // {
                //     bloomCmd.ReleaseTemporaryRT(bloomDownSampleIDs[i]);
                //     bloomCmd.ReleaseTemporaryRT(bloomUpSampleIDs[i]);
                // }
            }
        }
        
        #region Bloom
        void SetupBloom(Bloom bloomSettings)
        {
            int sampleCount = bloomSettings.maxSampleCount;
        
            if (sampleCount <= 0)
            {
                return;
            }
        
            //get the rt size and format
            int width = renderingData.cameraData.width;
            int height = renderingData.cameraData.height;
            RenderTextureFormat format = 
                renderingData.commonSettings.isHDR ? 
                RenderTextureFormat.DefaultHDR : 
                RenderTextureFormat.Default;
            FilterMode filter = FilterMode.Bilinear;

            //Filter
            bloomCmd.GetTemporaryRT(bloomFilterID, width, height, 0, filter, format);
            Draw(bloomCmd, ShaderDataID.cameraColorAttachmentId, bloomFilterID, PostShaderPass.filter);
        
            //DownSample
            bloomDownSampleIDs = new int[sampleCount];
            bloomUpSampleIDs = new int[sampleCount];
            int fromID = bloomFilterID;
            for (int i = 0; i < sampleCount; i++)
            {
                if (width <= 0 || height <= 0)
                {
                    sampleCount = i;
                    break;
                }
        
                bloomDownSampleIDs[i] = Shader.PropertyToID("_BloomDownSample" + i);
                bloomUpSampleIDs[i] = Shader.PropertyToID("_BloomUpSample" + i);
        
                bloomCmd.GetTemporaryRT(bloomDownSampleIDs[i], width, height, 0, filter, format);
                bloomCmd.GetTemporaryRT(bloomUpSampleIDs[i], width, height, 0, filter, format);
        
                Draw(bloomCmd, fromID, bloomDownSampleIDs[i], PostShaderPass.blurH);
                Draw(bloomCmd, bloomDownSampleIDs[i], bloomUpSampleIDs[i], PostShaderPass.blurV);
        
                fromID = bloomUpSampleIDs[i];
                //lower the RT's size to the half size, and set Bilinear filterMode, to ues the unity's mipmap
                //a single mipmap0 1x1 pixel is the mipmap1 2x2 pixel's average color
                //example ,when size/2 and blur size is 3x3, same as the blur size is 6x6
                width /= 2;
                height /= 2;
            }
        
            
        
            //UpSample
            for (int i = sampleCount - 1; i > 0; i--)
            {
                bloomCmd.SetGlobalTexture(ShaderDataID.TempRtSource1ID, bloomUpSampleIDs[i - 1]);
                Draw(bloomCmd, bloomUpSampleIDs[i], bloomUpSampleIDs[i - 1], PostShaderPass.upSample);
            }
            bloomCmd.SetGlobalTexture(ShaderDataID.TempRtSource1ID, bloomUpSampleIDs[0]);
            
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