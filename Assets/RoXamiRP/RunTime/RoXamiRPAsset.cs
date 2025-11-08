using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace RoXamiRP
{
    [CreateAssetMenu(fileName = "RoXamiRPAsset", menuName = "RoXamiRP/RoXamiRP Asset")]
    public class RoXamiRPAsset : RenderPipelineAsset, IDisposable
    {
        //===============================================================================//
        //===================================Property====================================//
        //===============================================================================//
        public RoXamiRendererAsset[] rendererAssets;

        public CommonSettings commonSettings;

        public AntialiasingSettings antialiasingSettings;

        public ShadowSettings shadowSettings = default;

        public ShaderAsset shaderAsset = new ShaderAsset();
        
        private readonly string[] antialiasingQualityKeywords = new string[]
        {
            "_AA_HIGH",
            "_AA_MIDDLE",
            "_AA_LOW"
        };
        
        private readonly string[] pcfKeyWords = new string[]
        {
            "_DIRECTIONAL_PCF0",
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7"
        };
        
        //===============================================================================//
        //==================================UpdateVoid===================================//
        //===============================================================================//
        private void Dispose(bool disposing)
        {
            CoreUtils.Destroy(shaderAsset.postMaterial);
            CoreUtils.Destroy(shaderAsset.blitFullScreenTriangleMaterial);
            CoreUtils.Destroy(shaderAsset.fxaaMaterial);
            CoreUtils.Destroy(shaderAsset.smaaMaterial);
            CoreUtils.Destroy(shaderAsset.deferredMaterial);
            CoreUtils.Destroy(shaderAsset.renderingDebugMaterial);
        }

        public void UpdateRoXamiRPSettings()
        {
            SetAAKeyWords();
            SetPcfSettings();
            SetDirectionalShadowsKeyword();
        }

        void SetDirectionalShadowsKeyword()
        {
            if (shadowSettings != null && shadowSettings.enableDirectionalShadows)
            {
                Shader.EnableKeyword(ShaderDataID.enableScreenSpaceShadowsID);
            }
            else
            {
                Shader.DisableKeyword(ShaderDataID.enableScreenSpaceShadowsID);
            }
        }

        void SetAAKeyWords()
        {
            if (antialiasingSettings != null)
            {
                AntialiasingQuality aaQuality = antialiasingSettings.antialiasingQuality;
                foreach (var key in antialiasingQualityKeywords)
                {
                    Shader.DisableKeyword(key);
                }

                if ((int)aaQuality >= antialiasingQualityKeywords.Length)
                {
                    Shader.EnableKeyword(antialiasingQualityKeywords[1]);
                }
                else
                {
                    Shader.EnableKeyword(antialiasingQualityKeywords[(int)aaQuality]);
                }
            }
        }

        void SetPcfSettings()
        {
            if (shaderAsset.screenSpaceShadowComputeShader && shadowSettings != null)
            {
                var cs = shaderAsset.screenSpaceShadowComputeShader;
                var filter = shadowSettings.directional.filter;
                
                foreach (var key in pcfKeyWords)
                {
                    cs.DisableKeyword(key);
                }
                cs.EnableKeyword(pcfKeyWords[(int)filter]);
            }
        }
        
        //===============================================================================//
        //=======================================Base====================================//
        //===============================================================================//

        public RoXamiRenderPipline m_RoXamiRP;
        
        protected override RenderPipeline CreatePipeline()
        {
            m_RoXamiRP = new RoXamiRenderPipline(this);
            
            return m_RoXamiRP;
        }

        private void OnEnable()
        {
            UpdateRoXamiRPSettings();
        }

        protected override void OnValidate()
        {
            UpdateRoXamiRPSettings();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    //===============================================================================//
    //=================================SettingClasses================================//
    //===============================================================================//
    [Serializable]
    public class AntialiasingSettings
    {
        public AntialiasingMode antialiasingMode;
        public AntialiasingQuality antialiasingQuality;
    }

    [Serializable]
    public class ShaderAsset
    {
        Material post;
        public Material postMaterial
        {
            get
            {
                if (!post)
                {
                    post = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/Post");
                }

                return post;
            }
        }

        Material deferred;
        public Material deferredMaterial
        {
            get
            {
                if (!deferred)
                {
                    deferred = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/DeferredToonLit");
                }
        
                return deferred;
            }
        }
        
        Material fxaa;
        public Material fxaaMaterial
        {
            get
            {
                if (!fxaa)
                {
                    fxaa = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/FXAA");
                }

                return fxaa;
            }
        }
        
        Material smaa;
        public Material smaaMaterial
        {
            get
            {
                if (!smaa)
                {
                    smaa = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/SMAA");
                }

                return smaa;
            }
        }
        
        Material  blitFullScreenTriangle;
        public Material blitFullScreenTriangleMaterial
        {
            get
            {
                if (! blitFullScreenTriangle)
                {
                    blitFullScreenTriangle = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/BlitFullScreenTriangle");
                }

                return  blitFullScreenTriangle;
            }
        }
        
        Material renderingDebug;
        public Material renderingDebugMaterial
        {
            get
            {
                if (!renderingDebug)
                {
                    renderingDebug = CoreUtils.CreateEngineMaterial("RoXamiRP/Hide/RenderingDebug");
                }

                return renderingDebug;
            }
        }

        public ComputeShader screenSpaceShadowComputeShader;
        public ComputeShader screenSpaceGIComputeShader;
    }
    
    [Serializable]
    public class CommonSettings
    {
        public bool enableGpuInstancing = false;
        public bool enableDynamicBatching = false;
        public bool isHDR;
    }

    [Serializable]
    public class ShadowSettings
    {
        public bool enableDirectionalShadows = true;
        
        [Min(1f)] public float maxDistance = 500f;

        [Range(0.001f, 1f)] public float distanceFade = 0.1f;

        public Directional directional = new Directional
        {
            atlasSize = MapSize._2048,
            filter = FilterModeSetting.PCF3x3,
            cascadeRatio1 = 0.3f,
            cascadeRatio2 = 0.6f,
            cascadeRatio3 = 0.8f,
            cascadeFade = 0.1f
        };
    }

    public enum FilterModeSetting
    {
        None,
        PCF3x3,
        PCF5x5,
        PCF7x7,
    }

    [Serializable]
    public struct Directional
    {
        public MapSize atlasSize;
        public FilterModeSetting filter;

        [Range(0f, 1f)] public float cascadeRatio1, cascadeRatio2, cascadeRatio3;

        public Vector3 CascadeRatios => new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);

        [Range(0.001f, 10f)] public float cascadeFade;
    }

    public enum MapSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192,
    }
}
