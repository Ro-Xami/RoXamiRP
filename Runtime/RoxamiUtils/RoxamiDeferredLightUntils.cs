using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RoxamiUtils
{
    public enum RoxamiToonDeferredPassInput
    {
        ToonLit,
    }
    
    public static class RoxamiDeferredLightUtils
    {
        private static Material m_DeferredToonMaterial;
        public static Material DeferredToonMaterial
        {
            get
            {
                if (!m_DeferredToonMaterial)
                {
                    m_DeferredToonMaterial = CoreUtils.CreateEngineMaterial(RoxamiShaderConst.deferredToonShaderName);
                }
                return m_DeferredToonMaterial;
            }
        }
        
        static readonly int roxamiAdditionalLightsCountID = Shader.PropertyToID("_RoxamiAdditionalLightsCount");
        
        public static void RenderToonDeferredLights(CommandBuffer cmd, RenderingData renderingData)
        {
            cmd.SetGlobalFloat(roxamiAdditionalLightsCountID, renderingData.lightData.additionalLightsCount);
            
            cmd.DrawMesh(RoxamiCommonUtils.FullScreenMesh, Matrix4x4.identity, DeferredToonMaterial, 0, (int)RoxamiToonDeferredPassInput.ToonLit);
        }

        public static void Dispose()
        {
            CoreUtils.Destroy(m_DeferredToonMaterial);
        }
    }
}