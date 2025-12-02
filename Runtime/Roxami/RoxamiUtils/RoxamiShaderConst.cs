using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

namespace RoxamiUtils
{
    public static class RoxamiCommonUtils
    {
        private static Mesh m_FullscreenMesh;
        public static Mesh FullScreenMesh
        {
            get
            {
                if (!m_FullscreenMesh)
                {
                    m_FullscreenMesh = DeferredLights.CreateFullscreenMesh();
                }
                return m_FullscreenMesh;
            }
        }
    }
    
    public static class RoxamiShaderConst
    {
        public const string deferredToonShaderName = "Hidden/RoxamiRP/ToonDeferred";
    }
}