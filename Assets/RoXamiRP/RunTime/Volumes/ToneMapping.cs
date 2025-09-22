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
            acesFilmKeyword = "_ACES_Film_ToneMapping",
            acesSimpleKeyword = "_ACES_Simple_ToneMapping",
            gtKeyword = "_GT_ToneMapping";

        public override void UpdateVolumeSettings()
        {
            if (isActive)
            {
                switch (toneMappingMode)
                {
                    case ToneMappingMode.None:
                        postMaterial.DisableKeyword(acesFilmKeyword);
                        postMaterial.DisableKeyword(acesSimpleKeyword);
                        postMaterial.DisableKeyword(gtKeyword);
                        break;
                    case ToneMappingMode.ACES_Film:
                        postMaterial.EnableKeyword(acesFilmKeyword);
                        postMaterial.DisableKeyword(acesSimpleKeyword);
                        postMaterial.DisableKeyword(gtKeyword);
                        break;
                    case ToneMappingMode.ACES_Simple:
                        postMaterial.DisableKeyword(acesFilmKeyword);
                        postMaterial.EnableKeyword(acesSimpleKeyword);
                        postMaterial.DisableKeyword(gtKeyword);
                        break;
                    case ToneMappingMode.GT:
                        postMaterial.DisableKeyword(acesFilmKeyword);
                        postMaterial.DisableKeyword(acesSimpleKeyword);
                        postMaterial.EnableKeyword(gtKeyword);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                postMaterial.DisableKeyword(acesFilmKeyword);
                postMaterial.DisableKeyword(acesSimpleKeyword);
                postMaterial.DisableKeyword(gtKeyword);
            }
        }
    }
}