using System.Collections.Generic;

namespace RoXamiRenderPipeline
{
    public enum RoXamiFeatureStack
    {
        ScreenSpacePlanarReflection,
        ScreenShotBlurUI,
        ScreenSpaceReflectionFeature,
        
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

        public void SetActive(RoXamiFeatureStack stack, bool active)
        {
            m_Features[(int)stack] = active;
        }

        public bool IsActive(RoXamiFeatureStack stack)
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