using System;
using UnityEngine;

namespace RoXamiRP
{
    public enum VolumeLightingType
    {
        RayMarching,
        RadioBlur
    }

    [Serializable]
    public class VolumeLightingRayMarchSettings
    {
        [Min(0f)] public float intensity = 1;
        [Min(0f)] public float power = 1f;
        [ColorUsage(false)] public Color volumeLightColor = Color.white;
    }
    
    [Serializable]
    public class VolumeLightingRadioBlurSettings
    {
        [Min(0f)] public float intensity = 1;
        [Min(0)] public float clampMax = 10f;
        [Range(0f, 2f)] public float threshold = 1.2f;
    }
    
    public class VolumeLighting : RoXamiVolumeBase
    {
        public VolumeLightingType type = VolumeLightingType.RadioBlur;
        
        [SerializeField]
        public VolumeLightingRayMarchSettings rayMarchingSettings = new VolumeLightingRayMarchSettings();
        
        [SerializeField]
        public VolumeLightingRadioBlurSettings radioBlurSettings = new VolumeLightingRadioBlurSettings();

        private readonly int volumeLightRayMarchPowerID = Shader.PropertyToID("_VolumeLighting_RayMarch_Power");
        private readonly int volumeLightRayMarchIntensityID = Shader.PropertyToID("_VolumeLighting_RayMarch_Intensity");
        
        private readonly int radioBlurVolumeLightingFilterParamsID = Shader.PropertyToID("_VolumeLighting_RadioBlur_FilterParams");
        
        public override void UpdateVolumeSettings()
        {
            switch (type)
            {
                case VolumeLightingType.RadioBlur:
                    SetRadioBlur();
                    break;
                
                case VolumeLightingType.RayMarching:
                    SetRayMarch();
                    break;
            }
        }

        void SetRayMarch()
        {
            if (rayMarchingSettings == null) return;

            Shader.SetGlobalFloat(volumeLightRayMarchIntensityID, Mathf.SmoothStep(0f, 1f, rayMarchingSettings.intensity));
            Shader.SetGlobalFloat(volumeLightRayMarchPowerID, rayMarchingSettings.power);
        }

        void SetRadioBlur()
        {
            if (radioBlurSettings == null) return;

            float linearThreshold = Mathf.GammaToLinearSpace(radioBlurSettings.threshold);
            float thresholdKnee = linearThreshold * 0.5f;
            Shader.SetGlobalVector(radioBlurVolumeLightingFilterParamsID, 
                new Vector4(radioBlurSettings.threshold, thresholdKnee, radioBlurSettings.clampMax));
        }
    }
}