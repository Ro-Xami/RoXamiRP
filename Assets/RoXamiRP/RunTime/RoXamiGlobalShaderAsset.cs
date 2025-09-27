using System;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    [CreateAssetMenu(fileName = "RoXamiRPAsset", menuName = "RoXamiRP/GlobalShaderAsset")]
    public class RoXamiGlobalShaderAsset : ScriptableObject
    {
        public LutGlobalShaderSettings lutSettings;
        public ActorGlobalShaderSettings actorSettings;
        
        [Serializable]
        public class LutGlobalShaderSettings
        {
            public Texture2D toonLut;
            public Texture2D actorCommonLut;
            public Texture2D actorFaceLut;
        }

        [Serializable]
        public class ActorGlobalShaderSettings
        {
            public Color rimColor = Color.white;
            public float rimOffest = 3f;
            public float rimThreshold = 0.05f;
            [Range(0, 1)] public float outlineWidth = 0.2f;
        }

        readonly int rimColorID = Shader.PropertyToID("_ActorRimColor");
        readonly int rimOffestID = Shader.PropertyToID("_ActorRimOffest");
        readonly int rimThresholdID = Shader.PropertyToID("_ActorRimThreshold");
        readonly int outlineWidthID = Shader.PropertyToID("_ActorOutlineWidth");

        private void OnEnable()
        {
            UpdateGlobalShader();
        }

        private void OnValidate()
        {
            UpdateGlobalShader();
        }

        public void UpdateGlobalShader()
        {
            if (actorSettings != null)
            {
                Shader.SetGlobalColor(rimColorID, actorSettings.rimColor);
                Shader.SetGlobalFloat(rimOffestID, actorSettings.rimOffest);
                Shader.SetGlobalFloat(rimThresholdID, actorSettings.rimThreshold);
                Shader.SetGlobalFloat(outlineWidthID, actorSettings.outlineWidth);
            }
        }
    }
}