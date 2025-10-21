using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    [ExcludeFromPreset]
    public abstract class RoXamiVolumeBase : ScriptableObject
    {
        public bool isActive = false;

        // protected Material postMaterial => RoXamiRPAsset.Instance.shaderAsset.postMaterial;
        // protected Material deferredMaterial => RoXamiRPAsset.Instance.shaderAsset.deferredMaterial;

        public abstract void UpdateVolumeSettings();
    }

    [CreateAssetMenu(menuName = "RoXamiRP/RoXamiVolumeAsset", fileName = "RoXamiVolumeAsset")]
    public class RoXamiVolumeAsset : ScriptableObject
    {
        
        public RoXamiGlobalGiData giData = new RoXamiGlobalGiData();
        public List<RoXamiVolumeBase> volumes = new List<RoXamiVolumeBase>();

        [Serializable]
        public class RoXamiGlobalGiData
        {
            public Texture2D giDiffuseTexture;
            public Texture2D giSpecularTexture;
        }

        public void UpdateAsset()
        {
            UpdateAssetVolumes();
            UpdateAssetGiData();
        }

        public void UpdateAssetVolumes()
        {
            foreach (var v in volumes)
            {
                v.UpdateVolumeSettings();
            }
        }

        public void UpdateAssetGiData()
        {
            if (giData != null)
            {
                Shader.SetGlobalTexture(ShaderDataID.reflectionTexture, giData.giSpecularTexture);
            }
        }

        public void UpdateAssetVolumeComponent<T>() where T : RoXamiVolumeBase
        {
            var volume = GetVolumeComponent<T>();
            if (volume && volume.isActive)
            {
                volume.UpdateVolumeSettings();
            }
        }

        public void SetAssetVolumeActive<T>(bool active) where T : RoXamiVolumeBase
        {
            var volume = GetVolumeComponent<T>();
            if (volume)
            {
                volume.isActive = active;
            }
        }

        public bool IsAssetVolumeActive<T>() where T : RoXamiVolumeBase
        {
            var volume = GetVolumeComponent<T>();
            if (!volume)
            {
                return false;
            }
            
            return volume.isActive;
        }

        public T GetVolumeComponent<T>() where T : RoXamiVolumeBase
        {
            foreach (var volume in volumes)
            {
                if (volume is T typedVolume)
                    return typedVolume;
            }
            return null;
        }
    }
}