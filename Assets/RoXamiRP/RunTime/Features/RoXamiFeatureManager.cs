using System.Collections.Generic;

namespace RoXamiRenderPipeline
{
    public enum RoXamiFeatureStack
    {
        ScreenSpacePlanarReflection,
        ScreenShotBlurUI,
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

        public void SetActive(RoXamiFeatureStack stack, bool active)
        {
            if (!m_Features.TryGetValue((int)stack, out var m_Active))
            {
                m_Active = active;
                m_Features.Add((int)stack, m_Active);
            }
            
            m_Active = active;
        }

        public bool isActive(RoXamiFeatureStack stack)
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