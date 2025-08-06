using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(RoXamiRendererAsset))]
public class RoXamiRendererAssetInspector : Editor
{
    private RoXamiRendererAsset asset;
    private SerializedProperty commonSettings, bloomSettings, roXamiRenderFeatures;

    private List<Type> renderFeatureTypes;
    private GenericMenu featureMenu;

    void OnEnable()
    {
        asset = target as RoXamiRendererAsset;
        commonSettings = serializedObject.FindProperty("commonSettings");
        roXamiRenderFeatures = serializedObject.FindProperty("roXamiRenderFeatures");

        CollectRenderFeatureTypes();
    }

    void CollectRenderFeatureTypes()
    {
        renderFeatureTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(RoXamiRenderFeature)) && !t.IsAbstract)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(commonSettings);

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Render Features", EditorStyles.boldLabel);

        for (int i = 0; i < asset.roXamiRenderFeatures.Count; i++)
        {
            var renderFeature = asset.roXamiRenderFeatures[i];
            if (renderFeature == null) continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Editor featureEditor = Editor.CreateEditor(renderFeature);
            featureEditor.OnInspectorGUI();

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

        serializedObject.ApplyModifiedProperties();
    }

    void ShowAddFeatureMenu()
    {
        featureMenu = new GenericMenu();
        foreach (var type in renderFeatureTypes)
        {
            featureMenu.AddItem(new GUIContent(type.Name), false, () =>
            {
                AddFeature(type);
            });
        }
        featureMenu.ShowAsContext();
    }

    void AddFeature(Type type)
    {
        string path = AssetDatabase.GetAssetPath(asset);

        var feature = ScriptableObject.CreateInstance(type) as RoXamiRenderFeature;
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
                UnityEngine.Object.DestroyImmediate(feature, true);
            }
        }

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(path);
        AssetDatabase.Refresh();
    }

}
