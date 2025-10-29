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
        [Range(2, 12)]
        public int sampleCount = 6;
        [Range(1, 6)]
        public int blurIterations = 3;
        [Min(1f)]
        public float blurSize = 1f;
    }
    
    public override void Create()
    {
        CreateRayMarchTypePass();
        CreateRadioBlurTypePass();
    }

    public override void AddRenderPasses(RoXamiRenderLoop renderLoop, ref RenderingData renderingData)
    {
        AddTypePass(renderLoop, renderingData);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(rayMarchMaterial);
    }
    
    private void AddTypePass(RoXamiRenderLoop renderLoop, RenderingData renderingData)
    {
        var volumeSettings = RoXamiVolume.Instance.GetVolumeComponent<VolumeLighting>();
        if (!volumeSettings || !volumeSettings.isActive)
        {
            return;
        }
        
        switch (volumeSettings.type)
        {
            case VolumeLightingType.RayMarching:
                if (rayMarchPass != null)
                {
                    rayMarchPass.volumeSettings = volumeSettings;
                    renderLoop.EnqueuePass(rayMarchPass);
                }
                break;
            
            case VolumeLightingType.RadioBlur:
                if (radioBlurPass != null && radioBlurSettings is not { blurIterations: < 1 })
                {
                    radioBlurPass.volumeSettings = volumeSettings;
                    renderLoop.EnqueuePass(radioBlurPass);
                }
                break;
        }
    }

    private void CreateRayMarchTypePass()
    {
        var shader = Shader.Find("RoXamiRP/Hide/RayMarchVolumeLighting");
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
    
    private void CreateRadioBlurTypePass()
    {
        var shader = Shader.Find("RoXamiRP/Hide/RadioBlurVolumeLighting");
        if (!shader)
        {
            return;
        }
        radioBlurMaterial = CoreUtils.CreateEngineMaterial(shader);
        
        if (radioBlurSettings == null)
        {
            return;
        }

        radioBlurPass = new RadioBlurPass(radioBlurSettings, radioBlurMaterial);
    }
    
    //================================================================================//
    //////////////////////////////////  RadioBlur  /////////////////////////////////////
    //================================================================================//

    #region RadioBlur
    class RadioBlurPass : RoXamiRenderPass
    {
        private const float maxVoL = 0.5f;
        private readonly RadioBlurSettings settings;
        readonly Material m_Material;
        private float VoL = -1f;
        public VolumeLighting volumeSettings;

        private const string bufferName = "VolumeLighting";
        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        private readonly int volumeLightIntensityID = Shader.PropertyToID("_VolumeLighting_RadioBlur_Intensity");
        private readonly int volumeLightingRadioBlurBlurParamsID = Shader.PropertyToID("_VolumeLighting_RadioBlur_BlurParams");
        private readonly int volumeLightBlurTexelSizeID = Shader.PropertyToID("_VolumeLighting_TexelSize");
        
        private readonly int radioBlurVolumeLightingTextureAID = Shader.PropertyToID("_VolumeLightingRadioBlurTextureA");
        private readonly int radioBlurVolumeLightingTextureBID = Shader.PropertyToID("_VolumeLightingRadioBlurTextureB");
        
        public RadioBlurPass(RadioBlurSettings settings, Material material)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            this.settings = settings;
            m_Material = material;
        }

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CalculateVoL(renderingData, out Vector3 mainLightDir, out VoL);
            
            if (!EnableVoL(VoL)) return;
            
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            GetRadioBlurTexture(renderingData);

            DrawFilter();

            DrawRadioBlurAndCombine(renderingData, mainLightDir, VoL);
            
            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
            if (!EnableVoL(VoL)) return;
            
            cmd.ReleaseTemporaryRT(radioBlurVolumeLightingTextureAID);
            cmd.ReleaseTemporaryRT(radioBlurVolumeLightingTextureBID);
        }

        void DrawFilter()
        {
            cmd.SetGlobalTexture(ShaderDataID.cameraDepthAttachmentId, ShaderDataID.cameraDepthCopyTextureID);
            DrawDontCareStore(cmd, 
                ShaderDataID.cameraColorAttachmentId, radioBlurVolumeLightingTextureAID,
                m_Material, 0);
        }

        void DrawRadioBlurAndCombine(RenderingData renderingData, Vector3 mainLightDir, float VoL)
        {
            Vector2 blurUV = GetDirectionalLightScreenUV(renderingData.cameraData.camera, mainLightDir);
            cmd.SetGlobalVector(volumeLightingRadioBlurBlurParamsID, 
                new Vector4(settings.sampleCount, settings.blurSize, blurUV.x, blurUV.y));

            float intensity = VoL * volumeSettings.radioBlurSettings.intensity;
            intensity *= intensity;
            cmd.SetGlobalFloat(volumeLightIntensityID, Mathf.SmoothStep(0, maxVoL, intensity));
            
            int width = renderingData.cameraData.width;
            int height = renderingData.cameraData.height;
            cmd.SetGlobalVector(volumeLightBlurTexelSizeID,
                new Vector4(width, height, 1 / (float)width, 1 / (float)height));
            
            for (int i = 0; i < settings.blurIterations; i++)
            {
                bool isAB = i % 2 == 0;
                bool isFinalDraw = i == settings.blurIterations - 1;
                
                DrawDontCareStore(cmd, 
                    //from
                    isAB? radioBlurVolumeLightingTextureAID : radioBlurVolumeLightingTextureBID, 
                    //to
                    isFinalDraw? ShaderDataID.cameraColorAttachmentId: 
                    isAB? radioBlurVolumeLightingTextureBID : radioBlurVolumeLightingTextureAID, 
                    //mat
                    m_Material, isFinalDraw ? 2: 1);
            }
        }
        
        Vector2 GetDirectionalLightScreenUV(Camera cam, Vector3 lightDirection)
        {
            Vector3 lightDir = -lightDirection.normalized;
            float distance = cam.farClipPlane * 0.5f;
            Vector3 worldPos = cam.transform.position + lightDir * distance;
            Vector3 screenPos = cam.WorldToViewportPoint(worldPos);
            
            return new Vector2(screenPos.x, screenPos.y);
        }

        void GetRadioBlurTexture(RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;

            cmd.GetTemporaryRT(radioBlurVolumeLightingTextureAID, 
                cameraData.cameraColorDescriptor, cameraData.cameraColorFilterMode);
            cmd.GetTemporaryRT(radioBlurVolumeLightingTextureBID, 
                cameraData.cameraColorDescriptor, cameraData.cameraColorFilterMode);
        }
        
        void CalculateVoL(RenderingData renderingData, out Vector3 mainLightDir, out float VoL)
        {
            var mainLight = renderingData.lightData.directionalLights;
            if (mainLight == null || mainLight.Count < 0 || !mainLight[0])
            {
                VoL = -1f;
                mainLightDir = Vector3.zero;
                return; 
            }
            
            mainLightDir = mainLight[0].transform.forward.normalized;
            Vector3 cameraDir = renderingData.cameraData.camera.transform.forward.normalized;

            VoL = -Vector3.Dot(cameraDir, mainLightDir);
        }

        bool EnableVoL(float VoL)
        {
            var needToDraw = VoL > maxVoL;
            //Debug.Log(needToDraw);
            return needToDraw;
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
        public VolumeLighting volumeSettings;
        
        public RayMarchPass(RayMarchSettings settings, int kernel, Material material, int passIndex)
        {
            this.settings = settings;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
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
        
        private readonly int volumeLightBlurTexelSizeID = Shader.PropertyToID("_VolumeLighting_TexelSize");
        

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
            
            GetSetRayMarchRtTarget(renderingData, out int width, out int height);
            DrawRayMarch(width, height);

            DrawBlur();
            
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

        void DrawBlur()
        {
            cmd.SetRenderTarget(ShaderDataID.cameraColorAttachmentId, 
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            DrawFullScreenTriangles(cmd, m_Material, passIndex);
        }
    }
    #endregion
}