using System;
using UnityEngine;

namespace RoXamiRP
{
    [Serializable]
    public enum FogMode
    {
        None,
        Linear,
        EXP,
        EXP2,
    }
    
    public class GlobalFog : RoXamiVolumeBase
    {
        [SerializeField] public FogMode fogMode = FogMode.None;
        [SerializeField] Color fogColor = Color.white;
        [SerializeField] float start = 0f;
        [SerializeField] float end = 300f;
        [SerializeField] [Range(0f, 1f)] float fogDensity = 0.1f;
        
        static readonly int fogParamsID = Shader.PropertyToID("_GlobalFogParams");
        static readonly int fogColorID = Shader.PropertyToID("_GlobalFogColor");

        static readonly string[] fogKeywords =
        {
            "_GlobalFog_None",
            "_GlobalFog_Linear",
            "_GlobalFog_EXP",
            "_GlobalFog_EXP2"
        };
        
        public override void UpdateVolumeSettings()
        {
            if (isActive && fogMode != FogMode.None)
            {
                SetKeyword(fogMode);
                RoXamiFeatureManager.Instance.SetFeatureActive(RoXamiFeatureStack.GlobalFog, true);
                Shader.SetGlobalVector(fogParamsID, new Vector4(start, end, fogDensity, 1f));
                Shader.SetGlobalColor(fogColorID, fogColor);
            }
            else
            {
                SetKeyword(FogMode.None);
                RoXamiFeatureManager.Instance.SetFeatureActive(RoXamiFeatureStack.GlobalFog, false);
            }
        }

        void SetKeyword(FogMode mode)
        {
            foreach (var key in fogKeywords)
            {
                Shader.DisableKeyword(key);
            }
            Shader.EnableKeyword(fogKeywords[(int)mode]);
        }
    }
}