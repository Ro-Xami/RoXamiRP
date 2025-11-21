using System;
using RoXamiRP;
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
        [Range(0, 10)] 
        public float blurSize = 1f;
        [Range(0f, 100f)]
        public float randomStrength = 0.1f;
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

    public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (!IsGameOrSceneCamera(renderingData.cameraData.camera)) return;
#endif

        AddTypePass(renderer, renderingData);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(rayMarchMaterial);
        radioBlurPass?.Dispose();
        radioBlurPass?.Dispose();
    }
    
    private void AddTypePass(RoXamiRenderer renderer, RenderingData renderingData)
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
                    renderer.EnqueuePass(rayMarchPass);
                }
                break;
            
            case VolumeLightingType.RadioBlur:
                if (radioBlurPass != null && radioBlurSettings is not { blurIterations: < 1 })
                {
                    radioBlurPass.volumeSettings = volumeSettings;
                    renderer.EnqueuePass(radioBlurPass);
                }
                break;
        }
    }

    private void CreateRayMarchTypePass()
    {
        var shader = Shader.Find("RoXamiRP/Hide/RayMarchVolumeLighting");
        if (!shader) return;
        
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
        if (!shader) return;
        
        radioBlurMaterial = CoreUtils.CreateEngineMaterial(shader);
        
        if (radioBlurSettings == null) return;

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
        private CommandBuffer cmd;
        RenderingData renderingData;

        private readonly int volumeLightIntensityID = Shader.PropertyToID("_VolumeLighting_RadioBlur_Intensity");
        private readonly int volumeLightingRadioBlurBlurParamsID = Shader.PropertyToID("_VolumeLighting_RadioBlur_BlurParams");
        private readonly int volumeLightBlurTexelSizeID = Shader.PropertyToID("_VolumeLighting_TexelSize");

        private const string radioBlurVolumeLightingRtAName = "_VolumeLightingRadioBlurTextureA";
        private const string radioBlurVolumeLightingRtBName = "_VolumeLightingRadioBlurTextureB";
        // static readonly int radioBlurVolumeLightingRtAID = Shader.PropertyToID(radioBlurVolumeLightingRtAName);
        // static readonly int radioBlurVolumeLightingRtBID = Shader.PropertyToID(radioBlurVolumeLightingRtBName);
        private RTHandle radioBlurVolumeLightingRtA;
        private RTHandle radioBlurVolumeLightingRtB;
        
        public RadioBlurPass(RadioBlurSettings settings, Material material)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            this.settings = settings;
            m_Material = material;
            
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CalculateVoL(renderingData, out Vector3 mainLightDir, out VoL);
            
            if (!EnableVoL(VoL)) return;
            
            this.renderingData = renderingData;
            cmd = renderingData.commandBuffer;
            
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                GetRadioBlurTexture(renderingData);

                DrawFilter();

                DrawRadioBlurAndCombine(renderingData, mainLightDir, VoL);
            }
            ExecuteCommandBuffer(context, cmd);
        }

        public override void Dispose()
        {
            radioBlurVolumeLightingRtA?.Release();
            radioBlurVolumeLightingRtB?.Release();
        }

        void DrawFilter()
        {
            cmd.SetGlobalTexture(ShaderDataID.cameraDepthCopyTextureID, renderingData.renderer.GetCameraDepthBufferRT());
            DrawDontCareStore(cmd, 
                renderingData.renderer.GetCameraColorBufferRT(), radioBlurVolumeLightingRtA,
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
                    isAB? radioBlurVolumeLightingRtA : radioBlurVolumeLightingRtB, 
                    //to
                    isFinalDraw? renderingData.renderer.GetCameraColorBufferRT(): 
                    isAB? radioBlurVolumeLightingRtB : radioBlurVolumeLightingRtA, 
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

            RoXamiRTHandlePool.GetRTHandleIfNeeded(
                ref radioBlurVolumeLightingRtA, 
                cameraData.cameraColorDescriptor, cameraData.cameraColorFilterMode, radioBlurVolumeLightingRtAName);
            
            RoXamiRTHandlePool.GetRTHandleIfNeeded(
                ref radioBlurVolumeLightingRtB, 
                cameraData.cameraColorDescriptor, cameraData.cameraColorFilterMode, radioBlurVolumeLightingRtBName);
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

            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }
        
        const string bufferName = "VolumeLighting";
        private CommandBuffer cmd;
        RenderingData renderingData;

        const string volumeLightingRtName = "_VolumeLightingTexture";
        static readonly int volumeLightingRtID = Shader.PropertyToID(volumeLightingRtName);
        private RTHandle volumeLightingRT;
        private readonly int texelSizeID = Shader.PropertyToID("_texelSize");
        private readonly int volumeLightingParamsID = Shader.PropertyToID("_volumeLightingParams");
        //private readonly int volumeLightDownSampleID = Shader.PropertyToID("_volumeLightDownSample");
        
        private readonly int volumeLightBlurTexelSizeID = Shader.PropertyToID("_VolumeLighting_TexelSize");
        

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            this.renderingData = renderingData;
            cmd = renderingData.commandBuffer;
            
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                GetSetRayMarchRtTarget(renderingData, out int width, out int height);
                DrawRayMarch(width, height);

                cmd.SetGlobalTexture(volumeLightingRtID, volumeLightingRT);
                
                DrawBlur();
            }

            ExecuteCommandBuffer(context, cmd);
        }

        public override void Dispose()
        {
            volumeLightingRT?.Release();
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

            RoXamiRTHandlePool.GetRTHandleIfNeeded(
                ref volumeLightingRT, descriptor, FilterMode.Bilinear, volumeLightingRtName);
            cmd.SetRenderTarget(volumeLightingRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        }

        void DrawRayMarch(int width, int height)
        {
            var cs = settings.computeShader;
            cmd.SetComputeVectorParam(cs, volumeLightingParamsID, 
                new Vector4(settings.stepSize, settings.maxStep, settings.maxRayLength, settings.randomStrength));
            cmd.SetComputeVectorParam(cs, texelSizeID,
                new Vector4(width, height, 1 / (float)width, 1 / (float)height));
            //cmd.SetComputeFloatParam(cs, volumeLightDownSampleID, settings.downSample);
            
            cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.cameraDepthCopyTextureID, renderingData.renderer.GetCameraDepthBufferRT());
            cmd.SetComputeTextureParam(cs, kernel, ShaderDataID.directionalShadowAtlasID, renderingData.cameraData.directionalLightShadowAtlas);
            cmd.SetComputeTextureParam(cs, kernel, volumeLightingRtID, volumeLightingRT);
                
            int threadGroupX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupY = Mathf.CeilToInt(height / 8.0f);
            cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
        }

        void DrawBlur()
        {
            cmd.SetRenderTarget(renderingData.renderer.GetCameraColorBufferRT(), 
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            DrawFullScreenTriangles(cmd, m_Material, passIndex);
        }
    }
    #endregion
}
