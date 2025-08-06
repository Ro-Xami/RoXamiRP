using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    [CustomEditor(typeof(RoXamiAdditionalCameraData))]

    public class RoXamiAdditionalCameraDataInspector : Editor
    {
        RoXamiAdditionalCameraData roAddCamData;

        public void OnEnable()
        {
            roAddCamData = target as RoXamiAdditionalCameraData;
        }

        public override void OnInspectorGUI()
        {
            RoXamiRenderAssetMenu();

            EditorGUILayout.Space(20);

            base.OnInspectorGUI();
        }

        void RoXamiRenderAssetMenu()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("RoXamiRendererAsset");

            RoXamiRPAsset rpAsset = (RoXamiRPAsset)GraphicsSettings.renderPipelineAsset;
            if (rpAsset == null)
            {
                return;
            }

            var rendererAssets = rpAsset.rendererAssets;

            string assetName =
                roAddCamData.additionalCameraData.roXamiRendererAssetID + 1 > rendererAssets.Length
                    ? "Null Renderer"
                    : rendererAssets[roAddCamData.additionalCameraData.roXamiRendererAssetID].name;

            if (GUILayout.Button(assetName))
            {
                RoXamiRenderAssetMenu(rendererAssets);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void RoXamiRenderAssetMenu(RoXamiRendererAsset[] rendererAssets)
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < rendererAssets.Length; i++)
            {
                var rendererAsset = rendererAssets[i];
                int assetIndex = i;
                menu.AddItem(new GUIContent(rendererAsset.name), false,
                    () => { roAddCamData.additionalCameraData.roXamiRendererAssetID = assetIndex; });
                menu.ShowAsContext();
            }
        }
    }
}