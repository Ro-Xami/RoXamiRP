using System;
using System.Collections.Generic;
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
                    var go = new GameObject("RoXami Volume")
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    m_Instance = go.AddComponent<RoXamiVolume>();
                }
                return m_Instance;
            }
        }
        
        public RoXamiVolumeAsset volumeAsset;

        public T GetVolumeComponent<T>() where T : RoXamiVolumeBase
        {
            if (volumeAsset != null)
            {
                volumeAsset.GetVolumeComponent<T>();
            }

            return null;
        }

        public void UpdateVolumesGiSettings()
        {
            if (volumeAsset)
            {
                volumeAsset.UpdateAsset();
            }
        }
        
        public void UpdateVolumesSettings()
        {
            if (volumeAsset)
            {
                volumeAsset.UpdateAssetVolumes();
            }
        }
        
        public void UpdateGiSettings()
        {
            if (volumeAsset)
            {
                volumeAsset.UpdateAssetGiData();
            }
        }
        
        public void UpdateVolumeSettings<T>() where T : RoXamiVolumeBase
        {
            if (volumeAsset)
            {
                volumeAsset.UpdateAssetVolumeComponent<T>();
            }
        }
        
        public void SetVolumeActive<T>(bool active) where T : RoXamiVolumeBase
        {
            if (volumeAsset)
            {
                volumeAsset.SetAssetVolumeActive<T>(active);
            }
        }
        
        public bool IsVolumeActive<T>() where T : RoXamiVolumeBase
        {
            if (!volumeAsset)
            {
                return false;
            }
            return volumeAsset.IsAssetVolumeActive<T>();
        }
        
        private void OnEnable()
        {
            m_Instance = this;
            UpdateVolumesGiSettings();
        }

        private void OnValidate()
        {
            m_Instance = this;
            UpdateVolumesGiSettings();
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            UpdateVolumesSettings();
        }
#endif
    }
}