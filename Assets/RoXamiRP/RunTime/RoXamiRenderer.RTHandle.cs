using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    partial class RoXamiRenderer
    {
        private RTHandle cameraColorActiveRT;
        private RTHandle cameraColorAttachmentART;
        private RTHandle cameraColorAttachmentBRT;
        
        private RTHandle cameraDepthAttachmentRT;
        private RTHandle cameraDepthCopyRT;

        private RenderTextureDescriptor cameraColorDescriptor = new RenderTextureDescriptor(1, 1)
        {
            depthBufferBits = 0,
            msaaSamples = 1,
            colorFormat = RenderTextureFormat.Default,
            //graphicsFormat = GraphicsFormat.
        };
        
        private RenderTextureDescriptor cameraDepthDescriptor = new RenderTextureDescriptor(1, 1)
        {
            depthBufferBits = 32,
            msaaSamples = 1,
            colorFormat = RenderTextureFormat.Depth
        };
        
        const FilterMode cameraColorFilterMode = FilterMode.Bilinear;
        const FilterMode cameraDepthFilterMode = FilterMode.Bilinear;
        
        private void SetUpCameraAttachment()
        {
            int width = renderingData.cameraData.width;
            int height = renderingData.cameraData.height;
            
            cameraColorDescriptor.width = width;
            cameraColorDescriptor.height = height;
            cameraColorDescriptor.colorFormat = renderingData.commonSettings.isHDR ? 
                RenderTextureFormat.DefaultHDR: 
                RenderTextureFormat.Default;

            cameraDepthDescriptor.width = width;
            cameraDepthDescriptor.height = height;

            renderingData.cameraData.cameraColorDescriptor = cameraColorDescriptor;
            renderingData.cameraData.cameraDepthDescriptor = cameraDepthDescriptor;
            renderingData.cameraData.cameraColorFilterMode = cameraColorFilterMode;
            renderingData.cameraData.cameraDepthFilterMode = cameraDepthFilterMode;

            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentART, cameraColorDescriptor,
                cameraColorFilterMode, ShaderDataID.cameraColorAttachmentBufferAName);
            cameraColorActiveRT = cameraColorAttachmentART;
            
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraDepthAttachmentRT, cameraDepthDescriptor,
                cameraDepthFilterMode, ShaderDataID.cameraDepthAttachmentBufferName);
        }
        
        public RTHandle GetCameraColorBufferRT()
        {
            return cameraColorActiveRT;
        }

        public RTHandle GetCameraDepthBufferRT()
        {
            return cameraDepthAttachmentRT;
        }
        
        public RTHandle GetCameraColorCopyRT()
        {
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentART, cameraColorDescriptor,
                cameraColorFilterMode, ShaderDataID.cameraColorAttachmentBufferAName);
            
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentBRT, cameraColorDescriptor,
                cameraColorFilterMode, ShaderDataID.cameraColorAttachmentBufferBName);
            
            cameraColorActiveRT = 
                cameraColorActiveRT.name == cameraColorAttachmentART.name?
                    cameraColorAttachmentBRT: cameraColorAttachmentART;

            return cameraColorActiveRT;
        }

        public RTHandle GetCameraDepthCopyRT()
        {
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraDepthCopyRT, cameraDepthDescriptor,
                cameraDepthFilterMode, ShaderDataID.cameraDepthCopyTextureName);

            return cameraDepthCopyRT;
        }
        
        public RTHandle GetSwitchCameraColorBufferRT()
        {
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentART, cameraColorDescriptor,
                cameraColorFilterMode, ShaderDataID.cameraColorAttachmentBufferAName);
            
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentBRT, cameraColorDescriptor,
                cameraColorFilterMode, ShaderDataID.cameraColorAttachmentBufferBName);
            
            cameraColorActiveRT = 
                cameraColorActiveRT.name == cameraColorAttachmentART.name?
                cameraColorAttachmentBRT: cameraColorAttachmentART;

            return cameraColorActiveRT;
        }
    }
}