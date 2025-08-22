using System;
using System.Collections.Generic;
using RoXamiRenderPipeline;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public struct RenderingData
    {
        public CullingResults cullingResults;
        public CameraData cameraData;
        public RoXamiRendererAsset rendererAsset;
        public CommonSettings commonSettings;
        public ShadowSettings shadowSettings;
        public ShaderAsset shaderAsset;
        public RuntimeData runtimeData;
        public AntialiasingSettings antialiasingSettings;
    }

    public struct CameraData
    {
        public Camera camera;
        public CameraRenderType cameraRenderType;
        public int width;
        public int height;
        public RenderTextureDescriptor cameraColorDescriptor;
        public RenderTextureDescriptor cameraDepthDescriptor;
        public FilterMode cameraColorFilterMode;
        public FilterMode cameraDepthFilterMode;
    }

    public struct RuntimeData
    {
        public bool isFinalBlit;
        public bool enableScreenSpaceShadows;
        public bool enablePostProcessing;
        public bool enableAntialiasing;
    }

    public static class ShaderDataID
    {
        public static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("ToonUnlit");
        public static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");

        public static readonly int cameraDepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
        public static readonly int cameraColorAttachmentId = Shader.PropertyToID("_CameraColorAttachment");
        public static readonly int cameraDepthCopyTextureID = Shader.PropertyToID("_CameraDepthTexture");
        public static readonly int cameraColorCopyTextureID = Shader.PropertyToID("_CameraColorTexture");
        
        public static readonly int postSource0Id = Shader.PropertyToID("_PostSource0");
        public static readonly int postSource1Id = Shader.PropertyToID("_PostSource1");
        
        public static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");

        public static readonly int directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
        public const string enableScreenSpaceShadowsID = "SCREENSPACE_SHADOWS";
    }
    
    public enum PostShaderPass
    {
        copy,
        filter,
        blurH,
        blurV,
        upSample,
        combine,
        finalBlit
    };
    
    public enum AntialiasingMode
    {
        FXAA_Quality,
        FXAA_Console,
        SMAA,
        Original,
    };

    public enum AntialiasingQuality
    {
        High,
        Middle,
        Low
    }
}
