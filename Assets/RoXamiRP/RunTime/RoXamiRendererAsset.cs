using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoXamiRenderPipeline
{
    [CreateAssetMenu(fileName = "RoXamiRenderer Asset", menuName = "RoXamiRP/RoXamiRenderer Asset")]

    public class RoXamiRendererAsset : ScriptableObject
    {
        public RendererSettings rendererSettings;
        
        [System.Serializable]
        public class RendererSettings
        {
            //public bool enableDeferredRendering;
            public bool enableLighting;
            public bool copyColorAfterSkybox;
            public bool copyDepthAfterOpaque;
        }
        
        public List<RoXamiRenderFeature> roXamiRenderFeatures = new List<RoXamiRenderFeature>(10);

        public static RoXamiRendererAsset defaultAsset;

        private void OnEnable()
        {
            if (defaultAsset == null)
            {
                defaultAsset = this;
            }
        }
    }
}
