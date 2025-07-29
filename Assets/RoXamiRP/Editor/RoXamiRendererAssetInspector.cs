using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoXamiRendererAsset))]
public class RoXamiRendererAssetInspector : Editor
{
    private RoXamiRendererAsset asset;

    private SerializedProperty
        commonSettings, bloomSettings, roXamiRenderFeatures;

    void OnEnable()
    {
        asset = target as RoXamiRendererAsset;
        commonSettings = serializedObject.FindProperty("commonSettings");
        bloomSettings = serializedObject.FindProperty("bloomSettings");
        roXamiRenderFeatures = serializedObject.FindProperty("roXamiRenderFeatures");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.PropertyField(commonSettings);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        EditorGUILayout.PropertyField(bloomSettings);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        for (int i = 0; i < asset.roXamiRenderFeatures.Count; i++)
        {
            var feature = asset.roXamiRenderFeatures[i];

            if (feature == null) continue;

            Editor editor = CreateEditor(feature);
    
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            feature.name = EditorGUILayout.TextField("Feature " + i, feature.name);

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                asset.roXamiRenderFeatures.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();

            editor.OnInspectorGUI(); // 显示每个 Feature 的自定义属性

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add RenderFeature"))
        {
            // 示例：添加一个默认类型的 Feature（你要根据你的实现实际创建对象）
            var newFeature = ScriptableObject.CreateInstance<ScreenSpacePlanarReflection>();
            newFeature.name = "New Feature";
            asset.roXamiRenderFeatures.Add(newFeature);
            AssetDatabase.AddObjectToAsset(newFeature, asset); // 嵌入到 RoXamiRendererAsset
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndVertical();
        
        serializedObject.ApplyModifiedProperties();
    }
}