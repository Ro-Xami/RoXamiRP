#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RoXamiCameraAutoAttach
{
    static RoXamiCameraAutoAttach()
    {
        EditorApplication.update += AttachToAllCameras;
    }

    static void AttachToAllCameras()
    {
        var cameras = Object.FindObjectsOfType<Camera>();
        foreach (var cam in cameras)
        {
            if (!cam.TryGetComponent<RoXamiAdditionalCameraData>(out _))
            {
                cam.gameObject.AddComponent<RoXamiAdditionalCameraData>();
            }
        }

        EditorApplication.update -= AttachToAllCameras;
    }
}
#endif