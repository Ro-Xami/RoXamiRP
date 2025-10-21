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
                    var volume = FindAnyObjectByType<RoXamiVolume>();
                    if (volume)
                    {
                        m_Instance = volume;
                    }
                    else
                    {
                        var go = new GameObject("RoXamiVolume")
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        m_Instance = go.AddComponent<RoXamiVolume>();
                    }
                }
                return m_Instance;
            }
        }
        
        public RoXamiVolumeAsset volumeAsset;

        public T GetVolumeComponent<T>() where T : RoXamiVolumeBase
        {
            if (volumeAsset != null)
            {
                return volumeAsset.GetVolumeComponent<T>();
            }

            return null;
        }

        public void Update()
        {
            UpdateVolumesSettings();
            UpdateGiSettings();
            UpdateShSettings();
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

        public void UpdateShSettings()
        {
            CoreRpToRoXamiRP.SHUtility.UploadToShader();
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
            Update();
        }

        private void OnValidate()
        {
            m_Instance = this;
            Update();
        }
    }
}