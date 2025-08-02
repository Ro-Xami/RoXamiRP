using UnityEngine;
using UnityEngine.Rendering;

public enum CameraRenderType
{
    Base,
    Overlay
}

public static class RoXamiCameraExtensions
{
    public static AdditionalCameraData GetRoXamiAdditionalCameraData(this Camera camera)
    {
        camera.TryGetComponent(out RoXamiAdditionalCameraData data);
        
        AdditionalCameraData additionalCameraData = data == null?
            RoXamiAdditionalCameraData.DefaultAdditionalCameraData:
            data.additionalCameraData ?? 
            RoXamiAdditionalCameraData.DefaultAdditionalCameraData;
        
        return additionalCameraData;
    }
}

[System.Serializable]
public class AdditionalCameraData
{
    [HideInInspector] public int roXamiRendererAssetID;
    public CameraRenderType cameraRenderType;
    public bool beOverLay;

    public AdditionalCameraData(int roXamiRendererAssetID, CameraRenderType cameraRenderType, bool beOverLay)
    {
        this.roXamiRendererAssetID = roXamiRendererAssetID;
        this.cameraRenderType = cameraRenderType;
        this.beOverLay = beOverLay;
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class RoXamiAdditionalCameraData : MonoBehaviour
{
    private static AdditionalCameraData defaultAdditionalCameraData;
    public static AdditionalCameraData DefaultAdditionalCameraData
    {
        get
        {
            if (defaultAdditionalCameraData == null)
            {
                if (!(RoXamiRPAsset)GraphicsSettings.renderPipelineAsset)
                {
                    Debug.LogError("There's no RoXami render pipeline asset in GraphicsSettings.");
                    return null;
                }
                
                var rpAsset = (RoXamiRPAsset)GraphicsSettings.renderPipelineAsset;
                defaultAdditionalCameraData = 
                    new AdditionalCameraData(0, CameraRenderType.Base, false);
            }
            return defaultAdditionalCameraData;
        }
    }
    
    [SerializeField]
    public AdditionalCameraData additionalCameraData;
}

