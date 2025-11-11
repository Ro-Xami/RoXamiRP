using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public enum CameraRenderType
    {
        Base,
        Overlay
    }

    public enum BackgroundType
    {
        Skybox,
        //Color,
        None
    }

    public static class RoXamiCameraExtensions
    {
        public static AdditionalCameraData GetRoXamiAdditionalCameraData(this Camera camera)
        {
            camera.TryGetComponent(out RoXamiAdditionalCameraData data);

            AdditionalCameraData additionalCameraData = !data
                ? RoXamiAdditionalCameraData.DefaultAdditionalCameraData
                : data.additionalCameraData ??
                  RoXamiAdditionalCameraData.DefaultAdditionalCameraData;

            return additionalCameraData;
        }
    }

    [Serializable]
    public class AdditionalCameraData
    {
        public RoXamiRendererAsset roXamiRendererAsset;
        public CameraRenderType cameraRenderType;
        public BackgroundType backgroundType;
        public List<Camera> cameraStack;
        public bool enableScreenSpaceShadows;
        public bool enablePostProcessing;
        public bool enableAntialiasing;

        private RoXamiRenderer m_Renderer;
        internal RoXamiRenderer renderer
        {
            get
            {
                if (m_Renderer == null)
                {
                    m_Renderer = new RoXamiRenderer();
                }
                return m_Renderer;
            }
        }

        public AdditionalCameraData(RoXamiRendererAsset roXamiRendererAsset, CameraRenderType cameraRenderType, List<Camera> cameraStack)
        {
            this.roXamiRendererAsset = roXamiRendererAsset;
            this.cameraRenderType = cameraRenderType;
            this.cameraStack = cameraStack;
        }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class RoXamiAdditionalCameraData : MonoBehaviour
    {
        private static AdditionalCameraData defaultAdditionalCameraData;
        public static AdditionalCameraData DefaultAdditionalCameraData
        {
            get
            {
                if (defaultAdditionalCameraData == null)
                {
                    if (!(RoXamiRPAsset)GraphicsSettings.renderPipelineAsset)
                    {
                        Debug.LogError("There's no RoXami render pipeline asset in GraphicsSettings.");
                        return null;
                    }

                    var rpAsset = (RoXamiRPAsset)GraphicsSettings.renderPipelineAsset;
                    defaultAdditionalCameraData =
                        new AdditionalCameraData(RoXamiRendererAsset.defaultAsset, CameraRenderType.Base, null);
                    defaultAdditionalCameraData.backgroundType = BackgroundType.Skybox;
                    defaultAdditionalCameraData.enableScreenSpaceShadows = true;
                    defaultAdditionalCameraData.enablePostProcessing = true;
                    defaultAdditionalCameraData.enableAntialiasing = true;
                }

                return defaultAdditionalCameraData;
            }
        }

        [SerializeField] public AdditionalCameraData additionalCameraData;
    }
}

