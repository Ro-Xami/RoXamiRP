using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace RoXamiRP
{
    [CustomEditor(typeof(RoXamiRendererAsset))]

    public class RoXamiRendererAssetInspector : Editor
    {
        private RoXamiRendererAsset asset;
        private SerializedProperty rendererSettings, bloomSettings, roXamiRenderFeatures;

        private List<Type> renderFeatureTypes;
        private GenericMenu featureMenu;
        
        private static readonly GUIStyle featureStyle = new GUIStyle();
        private Dictionary<RoXamiRenderFeature, bool> featureFoldoutStates = new Dictionary<RoXamiRenderFeature, bool>();

        void OnEnable()
        {
            asset = target as RoXamiRendererAsset;
            rendererSettings = serializedObject.FindProperty("rendererSettings");
            roXamiRenderFeatures = serializedObject.FindProperty("roXamiRenderFeatures");

            renderFeatureTypes = EditorTools.GetTypesInAssets<RoXamiRenderFeature>();
            featureStyle.normal.background = EditorTools.backgroundTexture;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(rendererSettings);

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginVertical(featureStyle);
            FeaturesWindow();
            EditorGUILayout.EndVertical();

            Undo.RecordObject(asset, "Change RoXami Renderer Asset");
            serializedObject.ApplyModifiedProperties();
        }

        private void FeaturesWindow()
        {
            EditorGUILayout.LabelField("Render Features", EditorStyles.boldLabel);

            for (int i = 0; i < asset.roXamiRenderFeatures.Count; i++)
            {
                var renderFeature = asset.roXamiRenderFeatures[i];
                if (renderFeature == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Initialize foldout state if not exists
                if (!featureFoldoutStates.ContainsKey(renderFeature))
                {
                    featureFoldoutStates[renderFeature] = true;
                }
                
                // Feature foldout
                featureFoldoutStates[renderFeature] = EditorGUILayout.Foldout(
                    featureFoldoutStates[renderFeature], 
                    renderFeature.GetType().Name, 
                    true, 
                    EditorStyles.foldoutHeader
                );
                
                if (featureFoldoutStates[renderFeature])
                {
                    Editor featureEditor = CreateEditor(renderFeature);
                    
                    EditorGUI.indentLevel++;
                    featureEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove Feature", GUILayout.Width(120)))
                {
                    string path = AssetDatabase.GetAssetPath(asset);
                    asset.roXamiRenderFeatures.RemoveAt(i);
                    featureFoldoutStates.Remove(renderFeature);
                    AssetDatabase.RemoveObjectFromAsset(renderFeature);
                    DestroyImmediate(renderFeature, true);

                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                    AssetDatabase.Refresh();
                    break;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add RenderFeature"))
            {
                ShowAddFeatureMenu();
            }

            if (GUILayout.Button("Remove All"))
            {
                RemoveUnusedFeatures();
            }

            EditorGUILayout.EndHorizontal();
        }

        void ShowAddFeatureMenu()
        {
            featureMenu = new GenericMenu();
            foreach (var type in renderFeatureTypes)
            {
                featureMenu.AddItem(new GUIContent(type.Name), false, () => { AddFeature(type); });
            }

            featureMenu.ShowAsContext();
        }

        void AddFeature(Type type)
        {
            string path = AssetDatabase.GetAssetPath(asset);

            var feature = CreateInstance(type) as RoXamiRenderFeature;
            if (feature == null)
            {
                return;
            }

            feature.name = type.Name;

            AssetDatabase.AddObjectToAsset(feature, path);
            asset.roXamiRenderFeatures.Add(feature);


            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(feature);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }


        void RemoveUnusedFeatures()
        {
            string path = AssetDatabase.GetAssetPath(asset);

            var featuresToRemove = new List<RoXamiRenderFeature>(asset.roXamiRenderFeatures);
            asset.roXamiRenderFeatures.Clear();

            foreach (var feature in featuresToRemove)
            {
                if (feature != null)
                {
                    AssetDatabase.RemoveObjectFromAsset(feature);
                    DestroyImmediate(feature, true);
                }
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }
    }
}
