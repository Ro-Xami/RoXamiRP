using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoXamiRP
{
    [CustomEditor(typeof(RoXamiVolume))]
    public class RoXamiVolumeInspector : Editor
    {
        RoXamiVolume volume;
        List<Type> volumeTypes = new List<Type>();
        
        private GenericMenu volumeMenu;
        private GUIStyle volumeStyle = new GUIStyle();
        private Dictionary<RoXamiVolumeBase, bool> volumeFoldoutStates = new Dictionary<RoXamiVolumeBase, bool>();
        
        void OnEnable()
        {
            volume = target as RoXamiVolume;

            volumeTypes = EditorTools.GetTypesInAssets<RoXamiVolumeBase>();
            
            volumeStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorTools.backgroundTexture
                }
            };
        }

        public override void OnInspectorGUI()
        {
            CreatVolume();

            if (!volume.volumeAsset)
            {
                return;
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(volumeStyle);
            GiDataWindow();
            EditorGUILayout.Space(5);
            
            VolumesWindow();
            EditorGUILayout.EndVertical();
            
            EditorUtility.SetDirty(volume);
            EditorUtility.SetDirty(volume.volumeAsset);
            Undo.RecordObject(volume, "Change Volume");
            Undo.RecordObject(volume.volumeAsset, "Change Volume");
        }

        private void GiDataWindow()
        {
            EditorGUILayout.LabelField("RoXami GI Data", EditorStyles.boldLabel);
            
            volume.volumeAsset.giData.giDiffuseTexture = EditorGUILayout.ObjectField("Gi Diffuse Texture", 
                volume.volumeAsset.giData.giDiffuseTexture, typeof(Cubemap), 
                true,GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Cubemap;
            
            volume.volumeAsset.giData.giSpecularTexture = EditorGUILayout.ObjectField("Gi Specular Texture", 
                volume.volumeAsset.giData.giSpecularTexture, typeof(Cubemap), 
                true,GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Cubemap;
        }

        private void CreatVolume()
        {
            EditorGUILayout.BeginHorizontal();
            volume.volumeAsset = EditorGUILayout.ObjectField(
                    "VolumeAsset", volume.volumeAsset, 
                    typeof(RoXamiVolumeAsset), true) 
                as RoXamiVolumeAsset;

            if (GUILayout.Button("Creat new"))
            {
                var volumeAsset = CreateInstance<RoXamiVolumeAsset>();
                AssetDatabase.CreateAsset(volumeAsset, "Assets/RoXamiVolumeBase.asset");
                volume.volumeAsset = volumeAsset;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void VolumesWindow()
        {
            EditorGUILayout.LabelField("RoXami Volumes", EditorStyles.boldLabel);

            for (int i = 0; i < volume.volumeAsset.volumes.Count; i++)
            {
                var v = volume.volumeAsset.volumes[i];
                if (v == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Initialize foldout state if not exists
                if (!volumeFoldoutStates.ContainsKey(v))
                {
                    volumeFoldoutStates[v] = true;
                }
                
                // Volume foldout
                volumeFoldoutStates[v] = EditorGUILayout.Foldout(
                    volumeFoldoutStates[v], 
                    v.GetType().Name, 
                    true, 
                    EditorStyles.foldoutHeader
                );
                
                if (volumeFoldoutStates[v])
                {
                    Editor featureEditor = CreateEditor(v);
                    
                    EditorGUI.indentLevel++;
                    featureEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    v.isActive = false;
                    string path = AssetDatabase.GetAssetPath(volume.volumeAsset);
                    volume.volumeAsset.volumes.RemoveAt(i);
                    volumeFoldoutStates.Remove(v);
                    AssetDatabase.RemoveObjectFromAsset(v);
                    DestroyImmediate(v, true);

                    EditorUtility.SetDirty(volume.volumeAsset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                    AssetDatabase.Refresh();
                    break;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Volume Component"))
            {
                ShowAddFeatureMenu();
            }
        }
        
        void ShowAddFeatureMenu()
        {
            volumeMenu = new GenericMenu();
            foreach (var type in volumeTypes)
            {
                volumeMenu.AddItem(new GUIContent(type.Name), false, () => { AddFeature(type); });
            }

            volumeMenu.ShowAsContext();
        }

        void AddFeature(Type type)
        {
            if (HasSameVolumes(type))
            {
                return;
            }
            
            string path = AssetDatabase.GetAssetPath(volume.volumeAsset);

            var v = CreateInstance(type) as RoXamiVolumeBase;
            if (v == null)
            {
                return;
            }

            v.name = type.Name;
            AssetDatabase.AddObjectToAsset(v, path);
            volume.volumeAsset.volumes.Add(v);
            EditorUtility.SetDirty(volume.volumeAsset);
            EditorUtility.SetDirty(v);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }

        bool HasSameVolumes(Type type)
        {
            foreach (var v in volume.volumeAsset.volumes)
            {
                if (v.GetType() == type)
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
