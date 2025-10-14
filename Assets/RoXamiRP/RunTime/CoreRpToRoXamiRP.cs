using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public static class CoreRpToRoXamiRP
    {
        public static class SHUtility
        {
            private static readonly int RoXamiRP_SHArID = Shader.PropertyToID("_RoXamiRP_SHAr");
            private static readonly int RoXamiRP_SHAgID = Shader.PropertyToID("_RoXamiRP_SHAg");
            private static readonly int RoXamiRP_SHAbID = Shader.PropertyToID("_RoXamiRP_SHAb");
            private static readonly int RoXamiRP_SHBrID = Shader.PropertyToID("_RoXamiRP_SHBr");
            private static readonly int RoXamiRP_SHBgID = Shader.PropertyToID("_RoXamiRP_SHBg");
            private static readonly int RoXamiRP_SHBbID = Shader.PropertyToID("_RoXamiRP_SHBb");
            private static readonly int RoXamiRP_SHCID = Shader.PropertyToID("_RoXamiRP_SHC");
            
            //暂时没找到具体的出处
            //https://www.jianshu.com/p/99f4775c93b9
            public static void UploadToShader()
            {
                SphericalHarmonicsL2 sh = RenderSettings.ambientProbe;
                
                // unity_SHAr.xyz / w = SH[0,3], SH[0,1], SH[0,2], SH[0,0]
                Shader.SetGlobalVector(RoXamiRP_SHArID, new Vector4(sh[0,3], sh[0,1], sh[0,2], sh[0,0]));
                Shader.SetGlobalVector(RoXamiRP_SHAgID, new Vector4(sh[1,3], sh[1,1], sh[1,2], sh[1,0]));
                Shader.SetGlobalVector(RoXamiRP_SHAbID, new Vector4(sh[2,3], sh[2,1], sh[2,2], sh[2,0]));

                // unity_SHBr.xyz / w = SH[0,4~7]
                Shader.SetGlobalVector(RoXamiRP_SHBrID, new Vector4(sh[0,4], sh[0,5], sh[0,6], sh[0,7]));
                Shader.SetGlobalVector(RoXamiRP_SHBgID, new Vector4(sh[1,4], sh[1,5], sh[1,6], sh[1,7]));
                Shader.SetGlobalVector(RoXamiRP_SHBbID, new Vector4(sh[2,4], sh[2,5], sh[2,6], sh[2,7]));

                // unity_SHC.xyz = SH[0,8~2,8]
                Shader.SetGlobalVector(RoXamiRP_SHCID, new Vector4(sh[0,8], sh[1,8], sh[2,8], 1.0f));
            }
        }
    }
}