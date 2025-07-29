using System;
using UnityEngine;
using UnityEngine.Rendering;
public struct RenderingData
{
    public CullingResults cullingResults;
    public CameraData cameraData;
    public RoXamiRendererAsset rendererAsset;
    public ShadowSettings shadowSettings;
    public ShaderAsset shaderAsset;
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

public static class ShaderDataID
{
    public static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("ToonUnlit");
    public static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");
    
    public static readonly int cameraDepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    public static readonly int cameraColorAttachmentId= Shader.PropertyToID("_CameraColorAttachment");
    public static readonly int cameraDepthCopyTextureID= Shader.PropertyToID("_CameraDepthTexture");
    public static readonly int cameraColorCopyTextureID= Shader.PropertyToID("_CameraColorTexture");
    public static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");

    public static readonly int directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
    public const string enableScreenSpaceShadowsID = "SCREENSPACE_SHADOWS";
}
