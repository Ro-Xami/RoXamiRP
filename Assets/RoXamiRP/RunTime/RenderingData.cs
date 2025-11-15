using System;
using System.Collections.Generic;
using RoXamiRP;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public struct RenderingData
    {
        public CullingResults cullingResults;
        public CameraData cameraData;
        public CommonSettings commonSettings;
        public ShadowSettings shadowSettings;
        public ShaderAsset shaderAsset;
        public AntialiasingSettings antialiasingSettings;
        public RendererSettings rendererSettings;
        public LightData lightData;
        public RuntimeData runtimeData;
        public RoXamiRenderer renderer;
        public CommandBuffer commandBuffer;
    }

    public struct LightData
    {
        public List<Light> directionalLights;
        public List<Light> additionalLights;
        public List<Light> pointLights;
        public List<Light> spotLights;
        public List<ShadowCasterLight> shadowCasterLights;
    }

    public struct ShadowCasterLight
    {
        public Light light;
        public int lightIndex;
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
        public RTHandle directionalLightShadowAtlas;
        public RTHandle[] GBufferRTs;
        public RenderTargetIdentifier[] GBufferTargets;
    }

    public struct RuntimeData
    {
        public bool isCameraStackFinally;
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

        //===========================================================================
        //Camera Color Depth
        
        public const string cameraColorAttachmentBufferAName = "_CameraColorAttachmentA";
        public const string cameraColorAttachmentBufferBName = "_CameraColorAttachmentB";
        
        public static readonly int cameraDepthCopyTextureID = Shader.PropertyToID("_CameraDepthTexture");
        public static readonly int cameraColorCopyTextureID = Shader.PropertyToID("_CameraColorTexture");
        
        public const string cameraDepthAttachmentBufferName = "_CameraDepthAttachment";
        public const string cameraDepthCopyTextureName = "_CameraDepthCopyRT";
        
        //===========================================================================
        //GBuffers
        public static readonly string[] gBufferNames = new string[]
        {
            "_GBuffer0",
            "_GBuffer1",
            "_GBuffer2",
            "_GBuffer3"
        };
        
        public static readonly int[] gBufferNameIDs = new int[]
        {
            Shader.PropertyToID(gBufferNames[(int)GBufferTye.Albedo]),
            Shader.PropertyToID(gBufferNames[(int)GBufferTye.Normal]),
            Shader.PropertyToID(gBufferNames[(int)GBufferTye.MRA]),
            Shader.PropertyToID(gBufferNames[(int)GBufferTye.Emission]),
        };

        //===========================================================================
        //Shadows
        public const string directionalShadowAtlasName = "_DirectionalShadowAtlas";
        public static readonly int directionalShadowAtlasID = Shader.PropertyToID(directionalShadowAtlasName);
        public const string enableScreenSpaceShadowsID = "SCREENSPACE_SHADOWS";
        public const string enableScreenSpaceReflectionID = "SCREENSPACE_REFLECTION";
        
        //===========================================================================
        //Common
        public static readonly int TempRtSource0ID = Shader.PropertyToID("_TempRtSource0");
        public static readonly int TempRtSource1ID = Shader.PropertyToID("_TempRtSource1");
        
        public static readonly int matrixInvVP_ID = Shader.PropertyToID("_RoXamiRP_MatrixInvVP");
        public static readonly int reflectionTexture = Shader.PropertyToID("_RoXamiRpReflectionTexture");
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
