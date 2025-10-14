using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    [ExecuteInEditMode]
    public class RoXamiVolume : MonoBehaviour
    {
        private static RoXamiVolume m_Instance;
        public static RoXamiVolume Instance
        {
            get
            {
                if (!m_Instance)
                {
                    GameObject go = new GameObject("RoXami Volume");
                    m_Instance = go.AddComponent<RoXamiVolume>();
                }
                return m_Instance;
            }
        }

        public bool isActive;

        [SerializeField] public RoXamiGiData giData;

        [Serializable]
        public struct RoXamiGiData
        {
            public Texture reflectionProbe;
            public Texture giTexture;
        }
        
        [SerializeField] public Bloom bloom;
        [SerializeField] public ToneMapping toneMapping;
        [SerializeField] public ColorAdjustment colorAdjustment;
        [SerializeField] public WhiteBalance whiteBalance;

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
            UpdateGiData();
            
            bloom?.UpdateVolumeSettings();
            toneMapping?.UpdateVolumeSettings();
            colorAdjustment?.UpdateVolumeSettings();
            whiteBalance?.UpdateVolumeSettings();
        }

        void UpdateGiData()
        {
            CoreRpToRoXamiRP.SHUtility.UploadToShader();
            Shader.SetGlobalTexture(ShaderDataID.reflectionTexture, giData.reflectionProbe);
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            UpdateVolumesSettings();
        }
#endif
    }
}