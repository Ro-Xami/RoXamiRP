using System;
using RoXamiRenderPipeline;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeLightingFeature : RoXamiRenderFeature
{
    //================================================================================//
    //////////////////////////////////  RayMarch   /////////////////////////////////////
    //================================================================================//
    [SerializeField]
    RayMarchSettings rayMarchSettings;
    
    RayMarchPass rayMarchPass;
    private Material rayMarchMaterial;
    private const int rayMarchPassIndex = 0;
    const string kernelName = "RayMarchVolumeLighting";
    
    [Serializable]
    class RayMarchSettings
    {
        public ComputeShader computeShader;
        //public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [Range(1, 4)] 
        public int downSample = 1;
        [Min(0)]
        public int maxStep = 100;
        [Range(0.0f, 0.1f)] 
        public float stepSize = 0.1f;
        [Min(0f)]
        public float maxRayLength = 100f;
        [Range(0f, 5f)] 
        public float rayStart = 2f;
        [Range(0, 10)] 
        public float blurSize = 1f;
    }
    
    //================================================================================//
    //////////////////////////////////  RadioBlur  /////////////////////////////////////
    //================================================================================//
    [SerializeField]
    RadioBlurSettings radioBlurSettings;
    
    RadioBlurPass radioBlurPass;
    Material radioBlurMaterial;
    
    [Serializable]
    class RadioBlurSettings
    {
        
    }
    
    public override void Create()
    {
        CreatRayMarchTypePass();
        CreateRadioBlurPass();
    }

    public override void AddRenderPasses(RoXamiRenderLoop renderLoop, ref RenderingData renderingData)
    {
        AddTypePass(renderLoop);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(rayMarchMaterial);
    }
    
    private void CreatRayMarchTypePass()
    {
        var shader = Shader.Find("RoXamiRP/Hide/VolumeLightingBlur");
        if (!shader)
        {
            return;
        }
        radioBlurMaterial = CoreUtils.CreateEngineMaterial(shader);
        
        if (radioBlurSettings == null)
        {
            return;
        }

        radioBlurPass = new RadioBlurPass(radioBlurSettings, rayMarchMaterial, 0);
    }
    
    private void AddTypePass(RoXamiRenderLoop renderLoop)
    {
        var volumeSettings = RoXamiVolume.Instance.GetVolumeComponent<VolumeLighting>();
        if (!volumeSettings || !volumeSettings.isActive)
        {
            return;
        }
        
        switch (volumeSettings.type)
        {
            case VolumeLightingType.RadioBlur:
                if (rayMarchPass != null)
                {
                    renderLoop.EnqueuePass(rayMarchPass);
                }
                break;
            
            case VolumeLightingType.RayMarching:
                if (radioBlurPass != null)
                {
                    radioBlurPass.volumeSettings = volumeSettings;
                    renderLoop.EnqueuePass(radioBlurPass);
                }
                break;
        }
    }

    private void CreateRadioBlurPass()
    {
        var shader = Shader.Find("");
        if (!shader)
        {
            return;
        }
        rayMarchMaterial = CoreUtils.CreateEngineMaterial(shader);
        
        if (rayMarchSettings == null || !rayMarchSettings.computeShader || rayMarchSettings.computeShader.FindKernel(kernelName) < 0)
        {
            return;
        }

        int kernel = rayMarchSettings.computeShader.FindKernel(kernelName);

        rayMarchPass = new RayMarchPass(rayMarchSettings, kernel, rayMarchMaterial, rayMarchPassIndex);
    }
    
    //================================================================================//
    //////////////////////////////////  RadioBlur  /////////////////////////////////////
    //================================================================================//

    #region RadioBlur
    class RadioBlurPass : RoXamiRenderPass
    {
        private RadioBlurSettings settings;
        Material m_Material;
        public VolumeLighting volumeSettings;
        public RadioBlurPass(RadioBlurSettings settings, Material material, int passIndex)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            this.settings = settings;
            m_Material = material;
        }

        private const string bufferName = "VolumeLighting";
        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
            
        }
    }
    #endregion

    
    //================================================================================//
    //////////////////////////////////  RayMarch   /////////////////////////////////////
    //================================================================================//
    #region RayMarchPass
    class RayMarchPass : RoXamiRenderPass
    {
        readonly RayMarchSettings settings;
        private readonly int kernel;
        private readonly Material m_Material;
        private readonly int passIndex;
        
        public RayMarchPass(RayMarchSettings settings, int kernel, Material material, int passIndex)
        {
            this.settings = settings;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;//settings.renderPassEvent;
            m_Material = material;
            this.passIndex = passIndex;
            this.kernel = kernel;
        }
        
        const string bufferName = "VolumeLighting";
        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        private readonly int volumeLightingTextureID = Shader.PropertyToID("_VolumeLightingTexture");
        private readonly int texelSizeID = Shader.PropertyToID("_texelSize");
        private readonly int volumeLightingParamsID = Shader.PropertyToID("_volumeLightingParams");
        private readonly int volumeLightDownSampleID = Shader.PropertyToID("_volumeLightDownSample");
        
        private readonly int volumeLightBlurTexelSizeID = Shader.PropertyToID("_volumeLightBlurTexelSize");

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
            
            GetSetRayMarchRtTarget(renderingData, out int width, out int height);
            DrawRayMarch(width, height);

            SetRenderTarget();
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
            cmd.ReleaseTemporaryRT(volumeLightingTextureID);
        }

        private void GetSetRayMarchRtTarget(RenderingData renderingData,  out int width, out int height)
        {
            var descriptor = renderingData.cameraData.cameraColorDescriptor;
            cmd.SetGlobalVector(volumeLightBlurTexelSizeID, 
                new Vector4(
                    descriptor.width, descriptor.height, 
                    1f / (float)descriptor.width * settings.blurSize, 
                    1f / (float)descriptor.height * settings.blurSize));
            
            descriptor.enableRandomWrite = true;
            descriptor.colorFormat = RenderTextureFormat.RFloat;
            // descriptor.useMipMap = true;
            // descriptor.autoGenerateMips = true;
            // descriptor.mipCount = 7;
            width = descriptor.width = Mathf.Max(2, descriptor.width / settings.downSample);
            height = descriptor.height = Mathf.Max(2, descriptor.height / settings.downSample);
                
            cmd.GetTemporaryRT(volumeLightingTextureID,descriptor, FilterMode.Bilinear);
            cmd.SetRenderTarget(volumeLightingTextureID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        }

        void DrawRayMarch(int width, int height)
        {
            var cs = settings.computeShader;
            cmd.SetComputeVectorParam(cs, volumeLightingParamsID, 
                new Vector4(settings.stepSize, settings.maxStep, settings.maxRayLength, settings.rayStart));
            cmd.SetComputeVectorParam(cs, texelSizeID,
                new Vector4(width, height, 1 / (float)width, 1 / (float)height));
            cmd.SetComputeFloatParam(cs, volumeLightDownSampleID, settings.downSample);
            
            cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.cameraDepthCopyTextureID, ShaderDataID.cameraDepthAttachmentId);
            cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.directionalShadowAtlasID, ShaderDataID.directionalShadowAtlasID);
            cmd.SetComputeTextureParam(cs, kernel, volumeLightingTextureID, volumeLightingTextureID);
                
            int threadGroupX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupY = Mathf.CeilToInt(height / 8.0f);
            cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
        }

        void SetRenderTarget()
        {
            cmd.SetRenderTarget(ShaderDataID.cameraColorAttachmentId, 
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            DrawFullScreenTriangles(cmd, m_Material, passIndex);
        }
    }
    #endregion
    
}