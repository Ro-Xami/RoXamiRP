using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoXamiRP
{
    public enum RendererType
    {
        Forward,
        Deferred,
    }
    
    [System.Serializable]
    public class RendererSettings
    {
        public RendererType rendererType = RendererType.Forward;
        public bool enableLighting;
        public bool copyColorAfterSkybox;
        public bool copyDepthAfterOpaque;
    }
    
    [CreateAssetMenu(fileName = "RoXamiRenderer Asset", menuName = "RoXamiRP/RoXamiRenderer Asset")]

    public class RoXamiRendererAsset : ScriptableObject
    {
        public RendererSettings rendererSettings = new RendererSettings();

        public List<RoXamiRenderFeature> roXamiRenderFeatures = new List<RoXamiRenderFeature>(10);

        internal static RoXamiRendererAsset m_DefaultAsset;
        public static RoXamiRendererAsset defaultAsset
        {
            set{}
            get
            {
                if (!m_DefaultAsset)
                {
                    m_DefaultAsset = CreateInstance<RoXamiRendererAsset>();
                    m_DefaultAsset.hideFlags = HideFlags.HideAndDontSave;
                }
                
                return m_DefaultAsset;
            }
        }

        private void OnEnable()
        {

        }
    }
}
