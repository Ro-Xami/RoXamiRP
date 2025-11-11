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
        public RendererSettings rendererSettings;

        public List<RoXamiRenderFeature> roXamiRenderFeatures = new List<RoXamiRenderFeature>(10);

        public static RoXamiRendererAsset defaultAsset;

        private void OnEnable()
        {
            if (!defaultAsset)
            {
                defaultAsset = this;
            }
        }
    }
}
