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
        public AntialiasingSettings antialiasingSettings;
        public LightData lightData;
        public RuntimeData runtimeData;
    }

    public struct LightData
    {
        public List<Light> directionalLights;
        public List<Light> additionalLights;
        public List<Light> pointLights;
        public List<Light> spotLights;
    }

    public struct CameraData
    {
        public Camera camera;
        public AdditionalCameraData additionalCameraData;
        public int width;
        public int height;
        public RenderTextureDescriptor cameraColorDescriptor;
        public RenderTextureDescriptor cameraDepthDescriptor;
        public FilterMode cameraColorFilterMode;
        public FilterMode cameraDepthFilterMode;
    }

    public struct RuntimeData
    {
        public bool isDeferred;
        public bool isFinalBlit;
        public bool isCastShadows;
        public bool isPost;
        public bool isAntialiasing;
    }
    
    public enum GBufferTye
    {
        Albedo,
        Normal,
        MRA,
        Emission
    }

    public static class ShaderDataID
    {
        public static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("ToonUnlit");
        public static readonly ShaderTagId toonLitShaderTagId = new ShaderTagId("ToonLit");
        public static readonly ShaderTagId toonGBufferShaderTagId = new ShaderTagId("ToonGBuffer");
        
        public static readonly ShaderTagId unityLitShaderTagId = new ShaderTagId("UniversalForward");
        public static readonly ShaderTagId unityUnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

        public static readonly int cameraDepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
        public static int cameraColorAttachmentId;
        
        public static readonly int cameraColorAttachmentAId = Shader.PropertyToID("_CameraColorAttachmentA");
        public static readonly int cameraColorAttachmentBId = Shader.PropertyToID("_CameraColorAttachmentB");
        
        public static readonly int[] gBufferNameIDs = new int[]
        {
            Shader.PropertyToID("_GBuffer0"),
            Shader.PropertyToID("_GBuffer1"),
            Shader.PropertyToID("_GBuffer2"),
            Shader.PropertyToID("_GBuffer3"),
        };
        
        public static readonly int cameraDepthCopyTextureID = Shader.PropertyToID("_CameraDepthTexture");
        public static readonly int cameraColorCopyTextureID = Shader.PropertyToID("_CameraColorTexture");
        
        public static readonly int TempRtSource0ID = Shader.PropertyToID("_TempRtSource0");
        public static readonly int TempRtSource1ID = Shader.PropertyToID("_TempRtSource1");
        
        public static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");
        public static readonly int reflectionTexture = Shader.PropertyToID("_RoXamiRpReflectionTexture");

        public static readonly int directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
        public const string enableScreenSpaceShadowsID = "SCREENSPACE_SHADOWS";
        public const string enableScreenSpaceReflectionID = "SCREENSPACE_REFLECTION";
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
        None,
    };

    public enum AntialiasingQuality
    {
        High,
        Middle,
        Low
    }
}
