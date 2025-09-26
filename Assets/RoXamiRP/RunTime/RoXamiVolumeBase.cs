using UnityEngine;

namespace RoXamiRenderPipeline
{
    [System.Serializable]
    public abstract class RoXamiVolumeBase
    {
        public bool isActive = false;

        // protected Material postMaterial => RoXamiRPAsset.Instance.shaderAsset.postMaterial;
        // protected Material deferredMaterial => RoXamiRPAsset.Instance.shaderAsset.deferredMaterial;

        public abstract void UpdateVolumeSettings();
    }
}