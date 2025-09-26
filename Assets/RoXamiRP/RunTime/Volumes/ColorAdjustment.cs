using System;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    [Serializable]
    public class ColorAdjustment : RoXamiVolumeBase
    {
        [Min(0f)] public float exposure = 0f;
        [Range(-0.5f, 0.5f)] public float hue = 0f;
        [Range(-10f, 10f)] public float saturation = 0f;
        [Range(-1f, 1f)] public float contrast = 0f;

        private const string colorAdjustmentsKeyword = "_ColorAdjustments";
        static readonly int colorAdjustmentFactorID = Shader.PropertyToID("_colorAdjustmentFactor");

        public override void UpdateVolumeSettings()
        {
            if (isActive)
            {
                Shader.EnableKeyword(colorAdjustmentsKeyword);
                SetupColorAdjustment();
            }
            else
            {
                Shader.DisableKeyword(colorAdjustmentsKeyword);
            }
        }
        
        void SetupColorAdjustment()
        {
            var factor = new Vector4( hue, saturation, exposure, contrast);
            Shader.SetGlobalVector(colorAdjustmentFactorID, factor);
        }
    }
}