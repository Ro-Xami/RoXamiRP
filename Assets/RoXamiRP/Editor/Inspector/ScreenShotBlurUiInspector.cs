using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RoXamiRenderPipeline
{
    [CustomEditor(typeof(ScreenShotBlurUI))]
    public class ScreenShotBlurUiInspector : Editor
    {
        private ScreenShotBlurUI blur;
        
        void OnEnable()
        {
            blur = (ScreenShotBlurUI)target;
            
            if (!blur.gameObject.TryGetComponent(out blur.imageComponent))
            {
                blur.imageComponent = blur.gameObject.AddComponent<Image>();
            }
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Debug", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (GUILayout.Button("Begin Blur"))
            {
                blur.BeginBlur();
            }

            if (GUILayout.Button("End Blur"))
            {
                blur.EndBlur();
            }
            
            GUILayout.EndHorizontal();
        }
    }
}