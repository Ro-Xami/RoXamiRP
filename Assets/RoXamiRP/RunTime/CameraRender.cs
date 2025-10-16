using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public partial class CameraRender
    {
        Camera camera;
        ScriptableRenderContext context;
        const string bufferName = "RoXami Render";

        private RenderingData renderingData = new RenderingData();
        private readonly RoXamiRenderer renderer = new RoXamiRenderer();

        readonly CommandBuffer cmd = new CommandBuffer
        {
            name = bufferName,
        };

        public void Render(
            ScriptableRenderContext scriptableRenderContext, Camera cameraIndex, AdditionalCameraData additionalCameraData,
            CommonSettings commonSettings, ShadowSettings shadowSettings, RoXamiRendererAsset rendererAsset, 
            ShaderAsset shaderAsset, AntialiasingSettings antialiasingSettings,
            bool isFinalBlit)
        {
            context = scriptableRenderContext;
            camera = cameraIndex;
            
            #region DrawEditor
            PrepareBuffer();
            PrepareForSceneWindow();
            #endregion
            
            SetCommonData();
            InitializedGlobalKeyword();
            
            SetUpRenderingData(
                commonSettings, shadowSettings, rendererAsset, 
                shaderAsset, additionalCameraData, antialiasingSettings, 
                isFinalBlit);
            SetUpCameraAttachment();

            cmd.BeginSample(SampleName);
            ExecuteBuffer();

            renderer.InitializedActiveRenderPass(rendererAsset, ref renderingData);
            renderer.CameraSetup(cmd, ref renderingData);
            renderer.ExecuteRoXamiRenderPass(context, ref renderingData);

            #region DrawEditor
            DrawUnsupportedShaders();
            DrawGizmos();
            DrawWire();
            #endregion

            renderer.CameraCleanUp();
            if (renderingData.runtimeData.isFinalBlit)
            {
                CleanUpCameraColorDepthRT();
            }

            cmd.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        private void SetUpCameraAttachment()
        {
            if (renderingData.cameraData.additionalCameraData.cameraRenderType != CameraRenderType.Base)
            {
                return;
            }
            
            int width = renderingData.cameraData.width;
            int height = renderingData.cameraData.height;
                
            RenderTextureDescriptor cameraColorDescriptor =
                new RenderTextureDescriptor(width, height)
                {
                    depthBufferBits = 0,
                    colorFormat = renderingData.commonSettings.isHDR ? 
                        RenderTextureFormat.DefaultHDR : 
                        RenderTextureFormat.Default
                };
            FilterMode cameraColorFilterMode = FilterMode.Bilinear;

            RenderTextureDescriptor cameraDepthDescriptor =
                new RenderTextureDescriptor(width, height)
                {
                    depthBufferBits = 32,
                    colorFormat = RenderTextureFormat.Depth
                };
            FilterMode cameraDepthFilterMode = FilterMode.Point;

            renderingData.cameraData.cameraColorDescriptor = cameraColorDescriptor;
            renderingData.cameraData.cameraDepthDescriptor = cameraDepthDescriptor;
            renderingData.cameraData.cameraColorFilterMode = cameraColorFilterMode;
            renderingData.cameraData.cameraDepthFilterMode = cameraDepthFilterMode;

            ShaderDataID.cameraColorAttachmentId = ShaderDataID.cameraColorAttachmentAId;
            //ShaderDataID.cameraDepthAttachmentId = ShaderDataID.cameraDepthAttachmentAId;
            cmd.GetTemporaryRT(ShaderDataID.cameraColorAttachmentAId, cameraColorDescriptor, cameraColorFilterMode);
            cmd.GetTemporaryRT(ShaderDataID.cameraDepthAttachmentId, cameraDepthDescriptor, cameraDepthFilterMode);
                
            cmd.GetTemporaryRT(ShaderDataID.cameraColorCopyTextureID, cameraColorDescriptor, cameraColorFilterMode);
            cmd.GetTemporaryRT(ShaderDataID.cameraDepthCopyTextureID, cameraDepthDescriptor, cameraDepthFilterMode);
            
            if (renderingData.runtimeData is { isFinalBlit: true, isAntialiasing: false, isPost: false })
            {
                return;
            }
            cmd.GetTemporaryRT(ShaderDataID.cameraColorAttachmentBId, cameraColorDescriptor, cameraColorFilterMode);
            //cmd.GetTemporaryRT(ShaderDataID.cameraDepthAttachmentBId, cameraDepthDescriptor, cameraDepthFilterMode);
        }

        void CleanUpCameraColorDepthRT()
        {
            cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorAttachmentAId);
            //cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthAttachmentAId);
            if (renderingData.runtimeData is { isPost: true, isAntialiasing: true} && 
                renderingData.cameraData.additionalCameraData.cameraRenderType == CameraRenderType.Base ||
                renderingData.cameraData.additionalCameraData.cameraRenderType != CameraRenderType.Base)
            {
                cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorAttachmentBId);
                cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthAttachmentId);
            }
            
            cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorCopyTextureID);
            cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthCopyTextureID);
        }

        void SetUpRenderingData(CommonSettings commonSettings, ShadowSettings shadowSettings, RoXamiRendererAsset rendererAsset,
            ShaderAsset shaderAsset, AdditionalCameraData additionalCameraData, AntialiasingSettings antialiasingSettings,
            bool isFinalBlit)
        {
            if (!camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                return;
            }

            p.shadowDistance = Mathf.Min(shadowSettings.maxDistance, camera.farClipPlane);
            CullingResults cullingResults = context.Cull(ref p);

            renderingData.commonSettings = commonSettings;
            renderingData.shadowSettings = shadowSettings;
            renderingData.rendererAsset = rendererAsset;
            renderingData.cullingResults = cullingResults;
            renderingData.shaderAsset = shaderAsset;
            renderingData.antialiasingSettings = antialiasingSettings;
            
            renderingData.cameraData.camera = camera;
            renderingData.cameraData.width = camera.pixelWidth;
            renderingData.cameraData.height = camera.pixelHeight;
            renderingData.cameraData.additionalCameraData = additionalCameraData;

            renderingData.runtimeData.isCastShadows = false;
            renderingData.runtimeData.isDeferred = false;
            renderingData.runtimeData.isFinalBlit = isFinalBlit;
            renderingData.runtimeData.isPost = 
                RoXamiVolume.Instance.isActive && 
                renderingData.cameraData.additionalCameraData.enablePostProcessing;
            renderingData.runtimeData.isAntialiasing = 
                renderingData.cameraData.additionalCameraData.enableAntialiasing && 
                renderingData.antialiasingSettings.antialiasingMode != AntialiasingMode.None;
        }

        void SetCommonData()
        {
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);

            Matrix4x4 vpMatrix = projMatrix * viewMatrix;
            Matrix4x4 invVP = vpMatrix.inverse;

            cmd.SetGlobalMatrix(ShaderDataID.matrixInvVP_ID, invVP);
        }

        void InitializedGlobalKeyword()
        {
            cmd.DisableShaderKeyword(ShaderDataID.enableScreenSpaceShadowsID);
            cmd.DisableShaderKeyword(ShaderDataID.enableScreenSpaceReflectionID);
        }
        
        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        #region CoreRP_Volume
        // bool HasAnyVolumeInView()
        // {
        //     var volumes = VolumeManager.instance.GetVolumes(camera.cullingMask);
        //     
        //     foreach (var volume in volumes)
        //     {
        //         if (volume.enabled && volume.isActiveAndEnabled)
        //             return true;
        //     }
        //     return false;
        // }
        #endregion
    }
}