using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace RoxamiUtils
{
    public static class RoxamiDisposeUtils
    {
        public static void RoxamiDispose()
        {
            RoxamiDeferredLightUtils.Dispose();
        }
    }
}