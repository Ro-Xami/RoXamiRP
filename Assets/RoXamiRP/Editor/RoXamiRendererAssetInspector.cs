using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace RoXamiRenderPipeline
{
    [CustomEditor(typeof(RoXamiRendererAsset))]

    public class RoXamiRendererAssetInspector : Editor
    {
        private RoXamiRendererAsset asset;
        private SerializedProperty rendererSettings, bloomSettings, roXamiRenderFeatures;

        private List<Type> renderFeatureTypes;
        private GenericMenu featureMenu;
        
        private static GUIStyle featureStyle = new GUIStyle();
        private static Texture2D m_FeatureBackgroundTexture;
        private static Texture2D featureBackgroundTexture
        {
            get
            {
                if (m_FeatureBackgroundTexture == null)
                {
                    m_FeatureBackgroundTexture = GetBackGroundTexture(new Color(0.17f, 0.17f, 0.2f, 1f));
                }
                return m_FeatureBackgroundTexture;
            }
        }

        void OnEnable()
        {
            asset = target as RoXamiRendererAsset;
            rendererSettings = serializedObject.FindProperty("rendererSettings");
            roXamiRenderFeatures = serializedObject.FindProperty("roXamiRenderFeatures");

            CollectRenderFeatureTypes();
            featureStyle.normal.background = featureBackgroundTexture;
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
                
                Editor featureEditor = CreateEditor(renderFeature);
                
                EditorGUILayout.LabelField(renderFeature.GetType().Name, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                featureEditor.OnInspectorGUI();
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove Feature", GUILayout.Width(120)))
                {
                    string path = AssetDatabase.GetAssetPath(asset);
                    asset.roXamiRenderFeatures.RemoveAt(i);
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
        
        void CollectRenderFeatureTypes()
        {
            renderFeatureTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(RoXamiRenderFeature)) && !t.IsAbstract)
                .ToList();
        }

        private static Texture2D GetBackGroundTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    texture.SetPixel(i, j, color);
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
