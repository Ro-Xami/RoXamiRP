using UnityEngine;
using UnityEngine.Rendering;

public enum CameraRenderType
{
    Base,
    Overlay
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class RoXamiAdditionalCameraData : MonoBehaviour
{
    [SerializeField] CameraRenderType cameraRenderType;
    [SerializeField] RoXamiRendererAsset roXamiRendererAsset;
}