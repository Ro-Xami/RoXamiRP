using System;
using UnityEngine;

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
        [SerializeField] public Bloom bloom;
        [SerializeField] public ToneMapping toneMapping;
        [SerializeField] public ColorAdjustment colorAdjustment;

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

// #if UNITY_EDITOR
//         private void Update()
//         {
//             if (!Application.isPlaying)
//             {
//                 m_Instance = this;
//                 UpdateVolumesSettings();
//             }
//         }
// #endif

        public void UpdateVolumesSettings()
        {
            bloom?.UpdateVolumeSettings();
            toneMapping?.UpdateVolumeSettings();
            colorAdjustment?.UpdateVolumeSettings();

        }
    }
}