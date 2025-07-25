// using System;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Profiling;
// using UnityEngine.Rendering;
//
// partial class CameraRender
// {
//     partial void DrawUnsupportedShaders();
//     partial void DrawGizmos();
//     partial void PrepareForSceneWindow();
//     partial void PrepareBuffer();
//
// #if UNITY_EDITOR
//     static ShaderTagId[] legacyShaderTagIds = {
//         new ShaderTagId("Always"),
//         new ShaderTagId("ForwardBase"),
//         new ShaderTagId("PrepassBase"),
//         new ShaderTagId("Vertex"),
//         new ShaderTagId("VertexLMRGBM"),
//         new ShaderTagId("VertexLM")
//     };
//
//     static Material errorMaterial;
//     string SampleName { get; set; }
//
//     partial void DrawUnsupportedShaders()
//     {
//         if (errorMaterial == null)
//         {
//             errorMaterial =
//                 new Material(Shader.Find("Hidden/InternalErrorShader"));
//         }
//         DrawingSettings drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
//         {
//             overrideMaterial = errorMaterial
//         };
//
//         for (int i = 1; i < legacyShaderTagIds.Length; i++)
//         {
//             drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
//         }
//
//         FilteringSettings filteringSettings = FilteringSettings.defaultValue;
//         context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
//     }
//
//     partial void DrawGizmos()
//     {
//         if (Handles.ShouldRenderGizmos())
//         {
//             context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
//             context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
//         }
//     }
//
//     partial void PrepareForSceneWindow()
//     {
//         if (camera.cameraType == CameraType.SceneView)
//         {
//             ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
//         }
//     }
//
//     partial void PrepareBuffer()
//     {
//         Profiler.BeginSample("RoXamiRP Editor Only");
//         cmd.name = SampleName = "RoXamiRP: " + camera.name;
//         Profiler.EndSample();
//     }
// #else
// 	const string SampleName = bufferName;
// #endif
// }