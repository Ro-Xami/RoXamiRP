using System.Collections.Generic;
using UnityEngine.Device;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public enum RoXamiFeatureStack
    {
        ScreenSpacePlanarReflection,
        ScreenShotBlurUI,
        ScreenSpaceReflectionFeature,
        GlobalFog,
        
        RenderingDebug,
    }
    
    public class RoXamiFeatureManager
    {
        private static RoXamiFeatureManager m_Instance;
        public static RoXamiFeatureManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new RoXamiFeatureManager();
                }
                return m_Instance;
            }
        }

        private readonly Dictionary<int, bool> m_Features = new Dictionary<int, bool>();

        public void SetFeatureActive(RoXamiFeatureStack stack, bool active)
        {
            m_Features[(int)stack] = active;
        }

        public bool IsFeatureActive(RoXamiFeatureStack stack)
        {
            if (!m_Features.TryGetValue((int)stack, out var active))
            {
                m_Features.Add((int)stack, false);
                return false;
            }
            return active;
        }
    }
}