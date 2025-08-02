// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Rendering;
//
// [CustomEditor(typeof(Camera))]
// public class CameraDataInspector : Editor
// {
//     Camera camera;
//     RoXamiAdditionalCameraData roAddCamData;
//     public void OnEnable()
//     {
//         camera = target as Camera;
//
//         if (camera == null)
//         {
//             return;
//         }
//
//         roAddCamData = 
//             camera.GetComponent<RoXamiAdditionalCameraData>() == null ? 
//                 camera.gameObject.AddComponent<RoXamiAdditionalCameraData>() : 
//                 camera.GetComponent<RoXamiAdditionalCameraData>();
//     }
//     
//     public override void OnInspectorGUI()
//     {
//         RoXamiRenderAssetMenu();
//         
//         EditorGUILayout.Space(20);
//         
//         //roAddCamData.additionalCameraData.cameraRenderType = EditorGUILayout.be
//     }
//
//     void RoXamiRenderAssetMenu()
//     {
//         EditorGUILayout.BeginHorizontal();
//         
//         EditorGUILayout.LabelField("RoXamiRendererAsset");
//         
//         RoXamiRPAsset rpAsset = (RoXamiRPAsset)GraphicsSettings.renderPipelineAsset;
//         if (rpAsset == null)
//         {
//             return;
//         }
//         var rendererAssets = rpAsset.rendererAssets;
//         
//         string assetName = 
//             roAddCamData.additionalCameraData.roXamiRendererAssetID + 1 > rendererAssets.Length ? 
//                 "Null Renderer" : rendererAssets[roAddCamData.additionalCameraData.roXamiRendererAssetID].name;
//         
//         if (GUILayout.Button(assetName))
//         {
//             RoXamiRenderAssetMenu(rendererAssets);
//         }
//         
//         EditorGUILayout.EndHorizontal();
//     }
//     
//     private void RoXamiRenderAssetMenu(RoXamiRendererAsset[] rendererAssets)
//     {
//         GenericMenu menu = new GenericMenu();
//         for (int i = 0; i < rendererAssets.Length; i++)
//         {
//             var rendererAsset = rendererAssets[i];
//             int assetIndex = i;
//             menu.AddItem(new GUIContent(rendererAsset.name), false, () =>
//             {
//                 roAddCamData.additionalCameraData.roXamiRendererAssetID = assetIndex;
//                 
//             });
//             menu.ShowAsContext();
//         }
//     }
// }