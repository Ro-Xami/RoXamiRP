using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    [ExcludeFromPreset]
    public abstract class RoXamiRenderFeature : ScriptableObject, IDisposable
    {
        public bool isActive = true;
        
        public abstract void Create();

        public abstract void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData);

        protected bool IsGameOrSceneCamera(Camera camera)
        {
            return camera.cameraType == CameraType.Game || camera.cameraType == CameraType.SceneView;
        }

        public void OnEnable()
        {
            Create();
        }

        public void OnValidate()
        {
            Create();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}