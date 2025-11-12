using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public partial class RoXamiRenderer
    {
        internal void PrepareForSceneWindow(Camera editorCamera)
        {
#if UNITY_EDITOR
            if (editorCamera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(editorCamera);
            }
#endif
        }
        
        partial void DrawUnsupportedShaders();
        partial void DrawGizmos();
        partial void DrawWire();

#if UNITY_EDITOR
        static readonly ShaderTagId[] legacyShaderTagIds =
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };

        static Material errorMaterial;

        partial void DrawUnsupportedShaders()
        {
            if (errorMaterial == null)
            {
                errorMaterial =
                    new Material(Shader.Find("Hidden/InternalErrorShader"));
            }

            DrawingSettings drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
            {
                overrideMaterial = errorMaterial
            };

            for (int i = 1; i < legacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }

            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        }

        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }

        partial void DrawWire()
        {
            context.DrawWireOverlay(camera);
        }
#endif
    }
}