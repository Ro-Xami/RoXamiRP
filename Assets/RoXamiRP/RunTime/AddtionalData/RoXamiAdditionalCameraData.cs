using UnityEngine;
using UnityEngine.Rendering;

public enum CameraRenderType
{
    Base,
    Overlay
}

public static class RoXamiCameraExtensions
{
    public static RoXamiAdditionalCameraData GetRoXamiAdditionalCameraData(this Camera camera)
    {
        camera.TryGetComponent(out RoXamiAdditionalCameraData data);
        return data;
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class RoXamiAdditionalCameraData : MonoBehaviour
{
    [SerializeField] public CameraRenderType cameraRenderType;
    [SerializeField] public RoXamiRendererAsset roXamiRendererAsset;
}

