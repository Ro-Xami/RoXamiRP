using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(RoXamiAdditionalCameraData))]
public class RoXamiAdditionalCameraDataInspector : Editor
{
    RoXamiAdditionalCameraData additionalCameraData;
    public void OnEnable()
    {
        additionalCameraData = target as RoXamiAdditionalCameraData;
    }
    
    public override void OnInspectorGUI()
    {
        RoXamiRPAsset rpAsset = (RoXamiRPAsset)GraphicsSettings.renderPipelineAsset;
        if (rpAsset == null)
        {
            return;
        }
        var rendererAssets = rpAsset.rendererAssets;
        
        string assetName = 
            additionalCameraData.roXamiRendererAssetID + 1 > rendererAssets.Length ? 
            "Null Renderer" : rendererAssets[additionalCameraData.roXamiRendererAssetID].name;

        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("RoXamiRendererAsset");
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
            menu.AddItem(new GUIContent(rendererAsset.name), false, () =>
            {
                additionalCameraData.roXamiRendererAssetID = assetIndex;
                
            });
            menu.ShowAsContext();
        }
    }
}