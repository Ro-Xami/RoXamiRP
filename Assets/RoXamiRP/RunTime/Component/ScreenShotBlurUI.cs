using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace RoXamiRP
{
    [ExecuteAlways]
    public class ScreenShotBlurUI : MonoBehaviour
    {
        private Material m_Material;
        public Material material
        {
            get
            {
                if (m_Material == null)
                {
                    m_Material = CoreUtils.CreateEngineMaterial("RoXamiRP/UI/UIScreenBlur");
                }
                return m_Material;
            }
        }
        
        public Image imageComponent;
        
        public void BeginBlur()
        {
            ScreenShotBlurFeature.BeginBlur();
            SetBlurImageActive(true);
        }

        public void EndBlur()
        {
            ScreenShotBlurFeature.EndBlur();
            SetBlurImageActive(false);
        }

        public void SetBlurImageActive(bool active)
        {
            imageComponent.enabled = active;
        }

        void UpdateSettings()
        {
            if (!TryGetComponent(out imageComponent))
            {
                imageComponent = gameObject.AddComponent<Image>();
            }
            
            imageComponent.material = material;
        }
        
        private void OnEnable()
        {
            UpdateSettings();
            SetBlurImageActive(false);
        }

        private void OnValidate()
        {
            UpdateSettings();
        }

        private void OnDisable()
        {
            CoreUtils.Destroy(material);
            SetBlurImageActive(false);
        }
    }
}