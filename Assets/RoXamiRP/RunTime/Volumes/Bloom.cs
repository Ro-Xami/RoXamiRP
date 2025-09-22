using System;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    [System.Serializable]
    public class Bloom : RoXamiVolumeBase
    {
        [Min(0f)] public float intensity = 0f;
        [Min(0f)] public float clampMax = 5f;
        [Min(0f)] public float threshold = 0.9f;
        [Range(0f, 1f)] public float scatter = 0.7f;
        [Range(0, 10)] public int maxSampleCount = 5;

        private const string bloomKeyword = "_Bloom";
        private static readonly int bloomParam = Shader.PropertyToID("_bloomParam");
        private static readonly int bloomIntensity = Shader.PropertyToID("_bloomIntensity");
        
        public override void UpdateVolumeSettings()
        {
            //Set bloom Shader datas
            float linearThreshold = Mathf.GammaToLinearSpace(threshold);
            float thresholdKnee = linearThreshold * 0.5f; // Hardcoded soft knee
            postMaterial.SetVector(bloomParam, new Vector4(
                threshold, thresholdKnee, clampMax, scatter));
            postMaterial.SetFloat(bloomIntensity, intensity);

            if (isActive)
            {
                postMaterial.EnableKeyword(bloomKeyword);
            }
            else
            {
                postMaterial.DisableKeyword(bloomKeyword);
            }
        }
    }
}