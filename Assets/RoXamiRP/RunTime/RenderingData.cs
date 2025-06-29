using System;
using UnityEngine;
using UnityEngine.Rendering;
public struct RenderingData
{
    public int width;
    public int height;
    public Camera camera;
    public ScriptableRenderContext context;
    public CullingResults cullingResults;
    public RoXamiRenderer renderer;
    public ShadowSettings shadowSettings;
    public bool isGPUInstancing;
    public bool isDynamicBatching;
    public bool isHDR;
    public RenderTextureDescriptor cameraColorDescriptor;
    public RenderTextureDescriptor cameraDepthDescriptor;
    public FilterMode cameraColorFilterMode;
    public FilterMode cameraDepthFilterMode;
    public int cameraDepthAttachmentId;
    public int cameraColorAttachmentId;
    public int cameraDepthCopyTextureID;
    public int cameraColorCopyTextureID;
    public ScreenSpaceShadowsData screenSpaceShadowsData;
}

public struct ScreenSpaceShadowsData
{
    public int directionalShadowAtlasID;
    public Vector4 directionalLightShadowData;
    public Vector4[] cascadeCullingSpheres;
    public Matrix4x4[] directionalShadowMatrices;
    public Vector4 shadowDistanceFade;
}
