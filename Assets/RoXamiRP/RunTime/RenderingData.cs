using System;
using UnityEngine;
using UnityEngine.Rendering;
public struct RenderingData
{
    public CullingResults cullingResults;
    public CameraData cameraData;
    public RoXamiRendererAsset rendererAsset;
    public ShadowSettings shadowSettings;
    public ScreenSpaceShadowsData screenSpaceShadowsData;
}

public struct CameraData
{
    public Camera camera;
    public int width;
    public int height;
    public RenderTextureDescriptor cameraColorDescriptor;
    public RenderTextureDescriptor cameraDepthDescriptor;
    public FilterMode cameraColorFilterMode;
    public FilterMode cameraDepthFilterMode;
}

public struct ScreenSpaceShadowsData
{
    public int directionalShadowAtlasID;
    public Vector4 directionalLightShadowData;
    public Vector4[] cascadeCullingSpheres;
    public Matrix4x4[] directionalShadowMatrices;
    public Vector4 shadowDistanceFade;
}

public static class ShaderDataID
{
    public static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("ToonUnlit");
    public static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");
    
    public static readonly int cameraDepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    public static readonly int cameraColorAttachmentId= Shader.PropertyToID("_CameraColorAttachment");
    public static readonly int cameraDepthCopyTextureID= Shader.PropertyToID("_CameraDepthTexture");
    public static readonly int cameraColorCopyTextureID= Shader.PropertyToID("_CameraColorTexture");
    public static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");
}
