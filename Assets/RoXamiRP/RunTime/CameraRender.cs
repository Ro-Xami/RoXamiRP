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
            bool isFinalBlit, bool isAB)
        {
            context = scriptableRenderContext;
            camera = cameraIndex;

            PrepareBuffer();
            PrepareForSceneWindow();
            SetCommonData();

            SetUpRenderingData(
                commonSettings, shadowSettings, rendererAsset, 
                shaderAsset, additionalCameraData, antialiasingSettings, 
                isFinalBlit, isAB);

            if (renderingData.cameraData.additionalCameraData.cameraRenderType == CameraRenderType.Base)
            {
                SetUpCameraColorDepthRT();
            }

            SetUpCurrentCameraAttachment();

            renderer.InitializedActiveRenderPass(rendererAsset, ref renderingData);

            cmd.BeginSample(SampleName);
            ExecuteBuffer();

            renderer.CameraSetup(cmd, ref renderingData);
            renderer.ExecuteRoXamiRenderPass(context, ref renderingData);

            DrawUnsupportedShaders();
            DrawGizmos();

            renderer.CameraCleanUp();
            
            if (isFinalBlit)
            {
                CleanUpCameraColorDepthRT();
            }

            cmd.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        private void SetUpCurrentCameraAttachment()
        {
            if (renderingData.runtimeData.isAB)
            {
                renderingData.cameraData.cameraColorAttachmentId = ShaderDataID.cameraColorAttachmentAId;
                renderingData.cameraData.cameraDepthAttachmentId = ShaderDataID.cameraDepthAttachmentAId;
            }
            else
            {
                renderingData.cameraData.cameraColorAttachmentId = ShaderDataID.cameraColorAttachmentBId;
                renderingData.cameraData.cameraDepthAttachmentId = ShaderDataID.cameraDepthAttachmentBId;
            }
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        void CleanUpCameraColorDepthRT()
        {
            cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorAttachmentAId);
            cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthAttachmentAId);
            if (renderingData.runtimeData.isFinalBlit)
            {
                cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorAttachmentBId);
                cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthAttachmentBId);
            }
            
            cmd.ReleaseTemporaryRT(ShaderDataID.cameraColorCopyTextureID);
            cmd.ReleaseTemporaryRT(ShaderDataID.cameraDepthCopyTextureID);
        }

        void SetUpRenderingData(CommonSettings commonSettings, ShadowSettings shadowSettings, RoXamiRendererAsset rendererAsset,
            ShaderAsset shaderAsset, AdditionalCameraData additionalCameraData, AntialiasingSettings antialiasingSettings,
            bool isFinalBlit, bool isAB)
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
            
            renderingData.runtimeData.isFinalBlit = isFinalBlit;
            renderingData.runtimeData.isAB = isAB;
        }
        
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

        private void SetUpCameraColorDepthRT()
        {
            int width = renderingData.cameraData.width;
            int height = renderingData.cameraData.height;
            
            RenderTextureDescriptor cameraColorDescriptor =
                new RenderTextureDescriptor(width, height);
            cameraColorDescriptor.depthBufferBits = 0;
            cameraColorDescriptor.colorFormat = 
                renderingData.commonSettings.isHDR ? 
                RenderTextureFormat.DefaultHDR : 
                RenderTextureFormat.Default;
            FilterMode cameraColorFilterMode = FilterMode.Bilinear;

            RenderTextureDescriptor cameraDepthDescriptor =
                new RenderTextureDescriptor(width, height);
            cameraDepthDescriptor.depthBufferBits = 32;
            cameraDepthDescriptor.colorFormat = RenderTextureFormat.Depth;
            FilterMode cameraDepthFilterMode = FilterMode.Point;

            renderingData.cameraData.cameraColorDescriptor = cameraColorDescriptor;
            renderingData.cameraData.cameraDepthDescriptor = cameraDepthDescriptor;
            renderingData.cameraData.cameraColorFilterMode = cameraColorFilterMode;
            renderingData.cameraData.cameraDepthFilterMode = cameraDepthFilterMode;

            renderingData.cameraData.cameraColorAttachmentId = ShaderDataID.cameraColorAttachmentAId;
            renderingData.cameraData.cameraDepthAttachmentId = ShaderDataID.cameraDepthAttachmentAId;
            cmd.GetTemporaryRT(ShaderDataID.cameraColorAttachmentAId, cameraColorDescriptor, cameraColorFilterMode);
            cmd.GetTemporaryRT(ShaderDataID.cameraDepthAttachmentAId, cameraDepthDescriptor, cameraDepthFilterMode);

            if (!renderingData.runtimeData.isFinalBlit)
            {
                cmd.GetTemporaryRT(ShaderDataID.cameraColorAttachmentBId, cameraColorDescriptor, cameraColorFilterMode);
                cmd.GetTemporaryRT(ShaderDataID.cameraDepthAttachmentBId, cameraDepthDescriptor, cameraDepthFilterMode);
            }
            
            cmd.GetTemporaryRT(ShaderDataID.cameraColorCopyTextureID, cameraColorDescriptor, cameraColorFilterMode);
            cmd.GetTemporaryRT(ShaderDataID.cameraDepthCopyTextureID, cameraDepthDescriptor, cameraDepthFilterMode);
        }


        void SetCommonData()
        {
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);

            Matrix4x4 vpMatrix = projMatrix * viewMatrix;
            Matrix4x4 invVP = vpMatrix.inverse;

            cmd.SetGlobalMatrix(ShaderDataID.matrixInvVP_ID, invVP);
        }
    }
}