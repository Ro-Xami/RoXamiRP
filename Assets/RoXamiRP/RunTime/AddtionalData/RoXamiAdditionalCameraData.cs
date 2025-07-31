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
    
    // public static RoXamiRendererAsset GetRoXamiRendererAsset(this Camera camera)
    // {
    //     if (camera.TryGetComponent<RoXamiAdditionalCameraData>(out var data) && data.roXamiRendererAsset != null)
    //     {
    //         return data.roXamiRendererAsset;
    //     }
    //
    //     return RoXamiRendererAsset.defaultAsset;
    // }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class RoXamiAdditionalCameraData : MonoBehaviour
{
    public int roXamiRendererAssetID = 0;
    public CameraRenderType cameraRenderType;
}

