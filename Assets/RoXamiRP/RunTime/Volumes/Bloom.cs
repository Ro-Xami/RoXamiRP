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

        private const string bloomKeyword = "_Post_Bloom_ON";
        private static readonly int bloomParam = Shader.PropertyToID("_PostBloomParams");
        private static readonly int bloomIntensity = Shader.PropertyToID("_PostBloomIntensity");
        
        public override void UpdateVolumeSettings()
        {
            //Set bloom Shader datas
            float linearThreshold = Mathf.GammaToLinearSpace(threshold);
            float thresholdKnee = linearThreshold * 0.5f; // Hardcoded soft knee
            Shader.SetGlobalVector(bloomParam, new Vector4(
                threshold, thresholdKnee, clampMax, scatter));
            Shader.SetGlobalFloat(bloomIntensity, intensity);

            if (isActive)
            {
                Shader.EnableKeyword(bloomKeyword);
            }
            else
            {
                Shader.DisableKeyword(bloomKeyword);
            }
        }
    }
}