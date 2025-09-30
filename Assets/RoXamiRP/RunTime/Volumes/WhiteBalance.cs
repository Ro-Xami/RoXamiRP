using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    [Serializable]
    public class WhiteBalance : RoXamiVolumeBase
    {
        [Range(-100f, 100f)] public float temperature = 0f; 
        [Range(-100f, 100f)] public float tint = 0f;
        
        private const string whiteBalanceKeyWords = "_Post_WhiteBalance_ON";
        private readonly int whiteBalanceParamsID = Shader.PropertyToID("_Post_WhiteBalanceParams");
        
        public override void UpdateVolumeSettings()
        {
            if (isActive)
            {
                Shader.SetGlobalVector(
                    whiteBalanceParamsID, ColorUtils.ColorBalanceToLMSCoeffs(temperature, tint));
                
                Shader.EnableKeyword(whiteBalanceKeyWords);
            }
            else
            {
                Shader.DisableKeyword(whiteBalanceKeyWords);
            }
        }
        
    }
}