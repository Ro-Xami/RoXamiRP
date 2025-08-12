using System.Collections.Generic;
using System.Linq;
using RoXamiRenderPipeline;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(Camera))]
public class CameraDataInspector : Editor
{
    private CameraProjection projection = CameraProjection.Perspective;
    enum CameraProjection
    {
        Perspective,
        Orthographic
    }
    
    private SensorPreset sensorPreset = SensorPreset.Custom;
    enum SensorPreset
    {
        Custom,
        _8mm,
        _16mm,
        _35mm,
        _70mm,
        _2over3Inch,
        _1Inch,
        Super35,
        APSC,
        FullFrame
    }

    Camera camera;
    private RoXamiAdditionalCameraData roXamiAdditionalCameraData;
    
    public void OnEnable()
    {
        camera = target as Camera;

        if (camera == null)
        {
            return;
        }

        if (!camera.TryGetComponent<RoXamiAdditionalCameraData>(out roXamiAdditionalCameraData))
        {
            roXamiAdditionalCameraData = camera.gameObject.AddComponent<RoXamiAdditionalCameraData>();
        }

        if (roXamiAdditionalCameraData.additionalCameraData == null)
        {
            roXamiAdditionalCameraData.additionalCameraData =
                new AdditionalCameraData(0, CameraRenderType.Base, null);
        }
    }
    
    public override void OnInspectorGUI()
    {
        RoXamiAdditionalCameraDataGUI();
        
        RoXamiAdditionalCameraSettings();

        BaseGUI();
        
        PhysicalSettings();
    }

    private void PhysicalSettings()
    {
        Undo.RecordObject(camera, "Change Camera");
        Undo.RecordObject(roXamiAdditionalCameraData, "Change RoXami Additional Camera Data");
        
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Physical Camera Settings", EditorStyles.boldLabel);
        camera.usePhysicalProperties = EditorGUILayout.Toggle("Use Physical Properties", camera.usePhysicalProperties);
        if (camera.usePhysicalProperties)
        {
            DrawSensorType();
            camera.gateFit = (Camera.GateFitMode)EditorGUILayout.EnumPopup("Gate Fit", camera.gateFit);
            camera.sensorSize = EditorGUILayout.Vector2Field("Sensor Size", camera.sensorSize);
    
            camera.focalLength = EditorGUILayout.FloatField("Focal Length", camera.focalLength);
            camera.lensShift = EditorGUILayout.Vector2Field("Lens Shift", camera.lensShift);
        }
        EditorGUILayout.EndVertical();
    }

    private void BaseGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Base Camera Settings", EditorStyles.boldLabel);
        
        camera.cullingMask = EditorGUILayout.MaskField(
            "Culling Mask",
            camera.cullingMask,
            InternalEditorUtility.layers
        );
        
        camera.backgroundColor = EditorGUILayout.ColorField("Background Color", camera.backgroundColor);
        
        projection = (CameraProjection)EditorGUILayout.EnumPopup("Projection", projection);
        camera.orthographic = projection == CameraProjection.Orthographic;

        if (camera.orthographic)
        {
            camera.orthographicSize = EditorGUILayout.FloatField("Orthographic Size", camera.orthographicSize);
        }
        else
        {
            camera.fieldOfView = EditorGUILayout.Slider("Field Of View", camera.fieldOfView, 0.1f, 180f);
        }

        camera.nearClipPlane = EditorGUILayout.FloatField("Near Clip Plane", camera.nearClipPlane);
        camera.farClipPlane = EditorGUILayout.FloatField("Far Clip Plane", camera.farClipPlane);

        camera.nearClipPlane = camera.nearClipPlane < 0 ? 0 : camera.nearClipPlane;
        camera.farClipPlane = camera.farClipPlane < 0 ? 0 : camera.farClipPlane;
        
        //camera.rect = EditorGUILayout.Vector4Field("Viewport Rect", camera.rect);
        camera.targetTexture = (RenderTexture)EditorGUILayout.ObjectField(
            new GUIContent("Target Texture"),
            camera.targetTexture,
            typeof(RenderTexture),
            false,
            GUILayout.Height(EditorGUIUtility.singleLineHeight)
        );
        camera.useOcclusionCulling = EditorGUILayout.Toggle("Use Occlusion Culling", camera.useOcclusionCulling);
        
        EditorGUILayout.EndVertical();
    }

    private void RoXamiAdditionalCameraSettings()
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("RoXami Additional Camera Settings", EditorStyles.boldLabel);
        RoXamiRenderAssetMenu();
        var addData = roXamiAdditionalCameraData.additionalCameraData;
        addData.enablePostProcessing = 
            EditorGUILayout.Toggle("Enable Post-processing", addData.enablePostProcessing);
        addData.enableScreenSpaceShadows =
            EditorGUILayout.Toggle("Enable Screen-space-shadows", addData.enableScreenSpaceShadows);
        addData.enableAntialiasing = 
            EditorGUILayout.Toggle("Enable Antialiasing", addData.enableAntialiasing);
        
        EditorGUILayout.EndVertical();
    }

    private void RoXamiAdditionalCameraDataGUI()
    {
        var addData = roXamiAdditionalCameraData.additionalCameraData;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("RoXami Camera RenderType", EditorStyles.boldLabel);
        
        addData.cameraRenderType = 
            (CameraRenderType)EditorGUILayout.EnumPopup("Render Type", addData.cameraRenderType);

        if (addData.cameraRenderType == CameraRenderType.Base)
        {
            camera.depth = EditorGUILayout.FloatField("Render Sorting", camera.depth);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(10);
            CameraStackList(addData);
            EditorGUILayout.EndVertical();
        }
 
        EditorGUILayout.EndVertical();
    }

    private void CameraStackList(AdditionalCameraData addData)
    {
        if (addData.cameraStack != null && addData.cameraStack.Count > 0)
        {
            foreach (var stack in addData.cameraStack.ToList())
            {
                if (stack == null)
                {
                    Debug.LogError("Stack Camera is null.");
                    addData.cameraStack.Remove(stack);
                    continue;
                }
                if (stack.GetRoXamiAdditionalCameraData().cameraRenderType != CameraRenderType.Overlay)
                {
                    Debug.LogError("Camera:" + stack.name + " is not an overlay camera.");
                    addData.cameraStack.Remove(stack);
                    continue;
                }
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(stack, typeof(Camera), true);
                if (GUILayout.Button("Remove"))
                {
                    addData.cameraStack.Remove(stack);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Add Stack Camera"))
        {
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            List<Camera> overlayCameras = new List<Camera>();
            foreach (var cam in cameras)
            {
                if (cam.GetRoXamiAdditionalCameraData().cameraRenderType == CameraRenderType.Overlay)
                {
                    overlayCameras.Add(cam);
                }
            }
                
            GenericMenu cameraStackMenu = new GenericMenu();
            foreach (var cam in overlayCameras)
            {
                cameraStackMenu.AddItem(new GUIContent(cam.name), false, () =>
                {
                    bool isRepeat = false;
                    if (addData.cameraStack != null && addData.cameraStack.Count > 0)
                    {
                        foreach (var stack in addData.cameraStack)
                        {
                            isRepeat = stack == cam;
                        }
                    }

                    if (isRepeat)
                    {
                        return;
                    }

                    addData.cameraStack.Add(cam);
                });
            }
            cameraStackMenu.ShowAsContext();
        }
    }
    
    void RoXamiRenderAssetMenu()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("RoXamiRendererAsset");

        RoXamiRPAsset rpAsset = (RoXamiRPAsset)GraphicsSettings.renderPipelineAsset;
        if (rpAsset == null)
        {
            return;
        }

        var rendererAssets = rpAsset.rendererAssets;

        string assetName =
            roXamiAdditionalCameraData.additionalCameraData.roXamiRendererAssetID + 1 > rendererAssets.Length
                ? "Null Renderer"
                : rendererAssets[roXamiAdditionalCameraData.additionalCameraData.roXamiRendererAssetID].name;

        if (GUILayout.Button(assetName))
        {
            RoXamiRenderAssetMenu(rendererAssets);
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void RoXamiRenderAssetMenu(RoXamiRendererAsset[] rendererAssets)
    {
         GenericMenu menu = new GenericMenu();
         for (int i = 0; i < rendererAssets.Length; i++)
         {
             var rendererAsset = rendererAssets[i];
             int assetIndex = i;
             menu.AddItem(new GUIContent(rendererAsset.name), false,
                 () => { roXamiAdditionalCameraData.additionalCameraData.roXamiRendererAssetID = assetIndex; });
             menu.ShowAsContext();
         }
    }

    private void DrawSensorType()
    {
        sensorPreset = (SensorPreset)EditorGUILayout.EnumPopup("Sensor Type", sensorPreset);
        switch (sensorPreset)
        {
            case SensorPreset._8mm: camera.sensorSize = new Vector2(4.8f, 3.5f); break;
            case SensorPreset._16mm: camera.sensorSize = new Vector2(10.26f, 7.49f); break;
            case SensorPreset._35mm: camera.sensorSize = new Vector2(36f, 24f); break;
            case SensorPreset._70mm: camera.sensorSize = new Vector2(70.41f, 52.63f); break;
            case SensorPreset._2over3Inch: camera.sensorSize = new Vector2(8.8f, 6.6f); break;
            case SensorPreset._1Inch: camera.sensorSize = new Vector2(12.8f, 9.6f); break;
            case SensorPreset.Super35: camera.sensorSize = new Vector2(24.89f, 18.66f); break;
            case SensorPreset.APSC: camera.sensorSize = new Vector2(23.6f, 15.7f); break;
            case SensorPreset.FullFrame: camera.sensorSize = new Vector2(36f, 24f); break;
            case SensorPreset.Custom: default: break; // 保留用户自定义
        }
    }
}

[CustomEditor(typeof(RoXamiAdditionalCameraData))]
public class RoXamiAdditionalCameraDataInspector : Editor
{
    public override void OnInspectorGUI()
    { }
}