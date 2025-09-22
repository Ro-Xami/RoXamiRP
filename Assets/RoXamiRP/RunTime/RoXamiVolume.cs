using System;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    [System.Serializable]
    public abstract class RoXamiVolumeBase
    {
        public bool isActive = false;

        protected Material postMaterial => RoXamiRPAsset.Instance.shaderAsset.postMaterial;
        protected Material deferredMaterial => RoXamiRPAsset.Instance.shaderAsset.deferredMaterial;

        public abstract void UpdateVolumeSettings();
    }
    
    [ExecuteAlways]
    public class RoXamiVolume : MonoBehaviour
    {
        private static RoXamiVolume m_Instance;
        public static RoXamiVolume Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject go = new GameObject("RoXamiVolume");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    m_Instance = go.AddComponent<RoXamiVolume>();
                }
                return m_Instance;
            }
        }

        public bool isActive = false;
        [SerializeField] public Bloom bloom = new Bloom();
        [SerializeField] public ToneMapping toneMapping = new ToneMapping();
        [SerializeField] public ColorAdjustment colorAdjustment = new ColorAdjustment();

        private void OnEnable()
        {
            m_Instance = this;
            UpdateVolumesSettings();
        }

        private void OnValidate()
        {
            m_Instance = this;
            UpdateVolumesSettings();
        }

        public void UpdateVolumesSettings()
        {
            bloom.UpdateVolumeSettings();
            toneMapping.UpdateVolumeSettings();
            colorAdjustment.UpdateVolumeSettings();
        }
    }
}