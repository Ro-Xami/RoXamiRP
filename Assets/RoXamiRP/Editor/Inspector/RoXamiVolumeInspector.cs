using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoXamiRenderPipeline
{
    [CustomEditor(typeof(RoXamiVolume))]
    public class RoXamiVolumeInspector : Editor
    {
        RoXamiVolume volume;
        List<Type> volumeTypes = new List<Type>();
        
        private GenericMenu volumeMenu;
        private GUIStyle volumeStyle = new GUIStyle();
        
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

            var diffuse = volume.volumeAsset.giData.giDiffuseTexture;
            diffuse = EditorGUILayout.ObjectField("Gi Diffuse Texture", 
                diffuse, typeof(Texture2D), 
                false,GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture2D;
            
            var specular = volume.volumeAsset.giData.giSpecularTexture;
            specular = EditorGUILayout.ObjectField("Gi Specular Texture", 
                specular, typeof(Texture2D), 
                false,GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Texture2D;
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

                Editor featureEditor = CreateEditor(v);

                EditorGUILayout.LabelField(v.GetType().Name, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                featureEditor.OnInspectorGUI();
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    v.isActive = false;
                    string path = AssetDatabase.GetAssetPath(volume.volumeAsset);
                    volume.volumeAsset.volumes.RemoveAt(i);
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