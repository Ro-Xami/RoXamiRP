using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public enum ToneMappingMode
    {
        None,
        ACES_Film,
        ACES_Simple,
        GT
    }
    
    [Serializable]
    public sealed class ToneMapping : RoXamiVolumeBase
    {
        public ToneMappingMode toneMappingMode = ToneMappingMode.None;
        
        private const string 
            acesFilmKeyword = "_Post_AcesFilm_ON",
            acesSimpleKeyword = "_Post_AcesSimple_ON",
            gtKeyword = "_Post_GT_ON";

        public override void UpdateVolumeSettings()
        {
            if (isActive)
            {
                switch (toneMappingMode)
                {
                    case ToneMappingMode.None:
                        Shader.DisableKeyword(acesFilmKeyword);
                        Shader.DisableKeyword(acesSimpleKeyword);
                        Shader.DisableKeyword(gtKeyword);
                        break;
                    case ToneMappingMode.ACES_Film:
                        Shader.EnableKeyword(acesFilmKeyword);
                        Shader.DisableKeyword(acesSimpleKeyword);
                        Shader.DisableKeyword(gtKeyword);
                        break;
                    case ToneMappingMode.ACES_Simple:
                        Shader.DisableKeyword(acesFilmKeyword);
                        Shader.EnableKeyword(acesSimpleKeyword);
                        Shader.DisableKeyword(gtKeyword);
                        break;
                    case ToneMappingMode.GT:
                        Shader.DisableKeyword(acesFilmKeyword);
                        Shader.DisableKeyword(acesSimpleKeyword);
                        Shader.EnableKeyword(gtKeyword);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Shader.DisableKeyword(acesFilmKeyword);
                Shader.DisableKeyword(acesSimpleKeyword);
                Shader.DisableKeyword(gtKeyword);
            }
        }
    }
}