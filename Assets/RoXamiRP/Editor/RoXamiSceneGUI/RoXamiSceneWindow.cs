using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoXamiRP
{
    [InitializeOnLoad]
    public static class RoXamiSceneWindows
    {
        static Rect panelRect = new Rect(50, 5, 400, 250);
        private const int titleHeight = 20;
        static bool isDragging = false;
        static Vector2 dragOffset;
        
        static readonly GUIStyle labelStyle = new GUIStyle();
        
        static readonly List<RoXamiSceneWindowBase> windows = new List<RoXamiSceneWindowBase>();

        static RoXamiSceneWindows()
        {
            OnEnable();
            
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnEnable()
        {
            labelStyle.normal.background = LoadEditorImage("Assets/RoXamiRP/Editor/Image/window.png");

            GetWindows();

            foreach (var window in windows)
            {
                window.OnEnable();
            }
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            BeginWindow();

            foreach (var window in windows)
            {
                window.OnSceneView(panelRect.width, panelRect.height);
            }
            
            EndWindow();
        }

        private static void EndWindow()
        {
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private static void BeginWindow()
        {
            Handles.BeginGUI();
            HandleDragging();
            GUILayout.BeginArea(panelRect, labelStyle);
            GUILayout.Label("RoXamiRP Window", EditorStyles.boldLabel, GUILayout.Height(titleHeight));
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }

        static void HandleDragging()
        {
            Event e = Event.current;
            Rect dragRect = new Rect(panelRect.x, panelRect.y, panelRect.width, titleHeight);

            EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.Pan);

            if (e.type == EventType.MouseDown && dragRect.Contains(e.mousePosition))
            {
                isDragging = true;
                dragOffset = e.mousePosition - new Vector2(panelRect.x, panelRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDragging)
            {
                panelRect.position = e.mousePosition - dragOffset;
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                isDragging = false;
            }
        }
        
        private static void GetWindows()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(RoXamiSceneWindowBase)) && !t.IsAbstract).ToList();

            foreach (var type in types)
            {
                RoXamiSceneWindowBase window = Activator.CreateInstance(type) as RoXamiSceneWindowBase;
                windows.Add(window);
            }
        }

        private static Texture2D LoadEditorImage(string relativePath)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            if (!tex)
            {
                tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
            }
            return tex;
        }
    }

}