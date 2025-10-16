using UnityEngine;

namespace RoXamiRenderPipeline
{
    public class RoXamiRenderDebug : RoXamiSceneWindowBase
    {
        private string[] renderDebugKeywords =
        {
            "_Debug_None",
            "_Debug_Albedo",
            "_Debug_Normal",
            "_Debug_Metallic",
            "_Debug_Roughness",
            "_Debug_Ao",
            "_Debug_Emission",
            "_Debug_GiDiffuse",
            "_Debug_GiSpecular",
            "_Debug_Shadow"
        };
        
        public override void OnEnable()
        {
            EnableKeyWord(0);
        }

        

        public override void OnSceneView(float width, float height)
        {
            for (int i = 0; i < renderDebugKeywords.Length; i++)
            {
                if (GUILayout.Button(renderDebugKeywords[i]))
                {
                    EnableKeyWord(i);
                }
            }
        }
        
        private void EnableKeyWord(int i)
        {
            foreach (var keyword in renderDebugKeywords)
            {
                Shader.DisableKeyword(keyword);
            }

            if (i > renderDebugKeywords.Length)
            {
                return;
            }
            Shader.EnableKeyword(renderDebugKeywords[i]);
        }
    }
}