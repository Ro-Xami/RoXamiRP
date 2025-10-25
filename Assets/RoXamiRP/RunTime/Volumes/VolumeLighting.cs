using UnityEngine;

namespace RoXamiRenderPipeline
{
    public class VolumeLighting : RoXamiVolumeBase
    {
        [Range(0f, 0.001f)] public float intensity = 1;
        [Min(0f)] public float power = 1f;
        [ColorUsage(false)] public Color volumeLightColor = Color.white;
        
        private readonly int volumeLightIntensityID = Shader.PropertyToID("_volumeLightIntensity");
        private readonly int volumeLightColorID = Shader.PropertyToID("_volumeLightColor");
        private readonly int volumeLightPowerID = Shader.PropertyToID("_volumeLightPower");
        
        public override void UpdateVolumeSettings()
        {
            Shader.SetGlobalColor(volumeLightColorID, volumeLightColor);
            Shader.SetGlobalFloat(volumeLightIntensityID, intensity);
            Shader.SetGlobalFloat(volumeLightPowerID, power);
        }
    }
}