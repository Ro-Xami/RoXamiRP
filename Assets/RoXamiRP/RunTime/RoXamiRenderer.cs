using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class RoXamiRenderer
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
        
        readonly List<RoXamiRenderPass> activePasses = 
            new List<RoXamiRenderPass>(32);

        private readonly LightingPass lightPass = 
            new LightingPass(RenderPassEvent.BeforeRenderingShadows + 10);

        private readonly RenderingPrePasses prePasses = 
            new RenderingPrePasses(RenderPassEvent.BeforeRenderingPrePasses + 10);

        // private readonly GBufferPass gBufferPass = 
        //     new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer + 10);
        //
        // private readonly ScreenSpaceShadowsPass ssShadowsPass =
        //     new ScreenSpaceShadowsPass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 9);
        //
        // private readonly DeferredDiffusePass _deferredDiffusePass = 
        //     new DeferredDiffusePass(RenderPassEvent.BeforeRenderingDeferredDiffuse + 10);

        private readonly ForwardOpaquePass forwardOpaquePass =
            new ForwardOpaquePass(RenderPassEvent.BeforeRenderingOpaques + 10);

        private readonly DrawSkyboxPass drawSkyboxPass = 
             new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox + 10);

        private readonly CopyCameraColorPass copyCameraColorPass =
            new CopyCameraColorPass(RenderPassEvent.AfterRenderingSkybox);
        
        private readonly CopyCameraDepthPass copyCameraDepthPass =
            new CopyCameraDepthPass(RenderPassEvent.AfterRenderingSkybox);

        private readonly ForwardTransparentPass forwardTransparentPass =
            new ForwardTransparentPass(RenderPassEvent.BeforeRenderingTransparents + 10);

        private readonly PostPass postPass = 
            new PostPass(RenderPassEvent.BeforeRenderingPostProcessing + 10);

        private readonly AntialiasingPass antialiasingPass = 
            new AntialiasingPass(RenderPassEvent.AfterRendering);

        private readonly FinalBlitPass finalBlitPass = 
            new FinalBlitPass(RenderPassEvent.AfterRendering + 1);

#if UNITY_EDITOR
        private readonly RenderingDebugPass renderingDebugPass =
            new RenderingDebugPass(RenderPassEvent.AfterRendering + 100);
#endif
        
        Camera camera;
        ScriptableRenderContext context;
        const string bufferName = "RoXami Render";
        private CommandBuffer cmd;
        internal RenderingData renderingData = new RenderingData();

        public void Render(ScriptableRenderContext context)
        {
            // #region DrawEditor
            // PrepareBuffer();
            // PrepareForSceneWindow();
            // #endregion
            
            SetCommonData();
            InitializedGlobalKeyword();
            SetUpCameraAttachment();

            RTHandles.ResetReferenceSize(renderingData.cameraData.width, renderingData.cameraData.height);

            cmd = CommandBufferPool.Get(bufferName);
            context.ExecuteCommandBuffer(cmd);
            CameraSetup(cmd, ref renderingData);
            ExecuteRoXamiRenderPass(context, ref renderingData);

            // #region DrawEditor
            // DrawUnsupportedShaders();
            // DrawGizmos();
            // DrawWire();
            // #endregion

            CameraCleanUp();

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }

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

            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorActiveRT, cameraColorDescriptor,
                ShaderDataID.cameraColorAttachmentBufferAName, cameraColorFilterMode);
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraDepthAttachmentRT, cameraDepthDescriptor,
                ShaderDataID.cameraDepthAttachmentBufferName, cameraDepthFilterMode);
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

        internal void InitializedActiveRenderPass(List<RoXamiRenderFeature> features)
        {
            activePasses.Clear();
            AddBaseRenderPasses(renderingData);
            AddRenderFeatures(features);
            SortStable(activePasses);
        }

        private void CameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            foreach (var pass in activePasses)
            {
                pass.SetUp(cmd, ref renderingData);
            }
        }

        private void ExecuteRoXamiRenderPass(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            foreach (var pass in activePasses)
            {
                //Debug.Log(renderingData.rendererAsset.name + ":" + pass.GetType().Name);
                pass.Execute(context, ref renderingData);
            }
        }

        private void CameraCleanUp()
        {
            foreach (var pass in activePasses)
            {
                pass.CleanUp();
            }
        }

        private void AddBaseRenderPasses(RenderingData renderingData)
        {
            if (renderingData.rendererAsset.rendererSettings.enableLighting)
            {
                activePasses.Add(lightPass);
            }
            activePasses.Add(prePasses);
            
            //=========================================================================
            //Deferred
            // if (renderingData.rendererAsset.rendererSettings.enableDeferredRendering && 
            //     renderingData.rendererAsset.rendererSettings.enableLighting)
            // {
            //     activePasses.Add(gBufferPass);
            //     activePasses.Add(deferredPass);
            //     activePasses.Add(ssShadowsPass);
            // }

            //=========================================================================
            //Forward
            activePasses.Add(forwardOpaquePass);
            
            if (renderingData.cameraData.additionalCameraData.backgroundType == BackgroundType.Skybox)
            {
                 activePasses.Add(drawSkyboxPass);
            }

            if (renderingData.rendererAsset.rendererSettings.copyColorAfterSkybox)
            {
                activePasses.Add(copyCameraColorPass);
            }

            if (renderingData.rendererAsset.rendererSettings.copyDepthAfterOpaque)
            {
                activePasses.Add(copyCameraDepthPass);
            }
            
            activePasses.Add(forwardTransparentPass);

            //=========================================================================
            //Post Antialiasing FinalBlit
            if (renderingData.runtimeData is { isPost: false, isAntialiasing: false, isCameraStackFinally: true })
            {
                activePasses.Add(finalBlitPass);
            }
            else
            {
                if (renderingData.runtimeData.isPost)
                {
                    activePasses.Add(postPass);
                }

                if (renderingData.runtimeData.isAntialiasing)
                {
                    activePasses.Add(antialiasingPass);
                }
            }
            
            #if UNITY_EDITOR

            if (RoXamiFeatureManager.Instance.IsFeatureActive(RoXamiFeatureStack.RenderingDebug) && 
                renderingData.cameraData.additionalCameraData.cameraRenderType == CameraRenderType.Base)
            {
                activePasses.Add(renderingDebugPass);
            }

            #endif
        }

        private void AddRenderFeatures(List<RoXamiRenderFeature> features)
        {
            foreach (var feature in features)
            {
                if (!feature || !feature.isActive)
                {
                    continue;
                }
                feature.AddRenderPasses(this, ref renderingData);
            }
        }

        internal void Dispose()
        {
            cameraColorAttachmentART?.Release();
            cameraColorAttachmentBRT?.Release();
            cameraDepthAttachmentRT?.Release();
        }

        private void SortStable(List<RoXamiRenderPass> list)
        {
            for (int i = 1; i < activePasses.Count; ++i)
            {
                RoXamiRenderPass curr = list[i];

                var j = i - 1;
                for (; j >= 0 && curr < list[j]; --j)
                    list[j + 1] = list[j];

                list[j + 1] = curr;
            }
        }
        
        public RTHandle GetCameraColorBufferRT()
        {
            return cameraColorActiveRT;
        }

        public RTHandle GetCameraDepthBufferRT()
        {
            return cameraColorActiveRT;
        }
        
        public RTHandle GetCameraColorCopyRT()
        {
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentART, cameraColorDescriptor,
                ShaderDataID.cameraColorAttachmentBufferAName, cameraColorFilterMode);
            
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentBRT, cameraColorDescriptor,
                ShaderDataID.cameraColorAttachmentBufferBName, cameraColorFilterMode);
            
            return cameraColorActiveRT == cameraColorAttachmentART?
                cameraColorAttachmentBRT: cameraColorAttachmentART;
        }

        public RTHandle GetCameraDepthCopyRT()
        {
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraDepthCopyRT, cameraDepthDescriptor,
                ShaderDataID.cameraDepthCopyTextureName, cameraDepthFilterMode);

            return cameraDepthCopyRT;
        }
        
        public RTHandle GetSwitchCameraColorBufferRT()
        {
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentART, cameraColorDescriptor,
                ShaderDataID.cameraColorAttachmentBufferAName, cameraColorFilterMode);
            
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref cameraColorAttachmentBRT, cameraColorDescriptor,
                ShaderDataID.cameraColorAttachmentBufferBName, cameraColorFilterMode);
            
            return cameraColorActiveRT == cameraColorAttachmentART?
                cameraColorAttachmentBRT: cameraColorAttachmentART;
        }

        private void EnqueuePass(RoXamiRenderPass pass)
        {
            activePasses.Add(pass);
        }
        
        internal void InitializedRenderingData( 
            ScriptableRenderContext context, 
            Camera camera, AdditionalCameraData additionalCameraData, 
            RoXamiRPAsset rpAsset,
            ScriptableCullingParameters cullingParameters, bool isCameraStackFinally)
        {
            renderingData.commonSettings = rpAsset.commonSettings;
            renderingData.shaderAsset = rpAsset.shaderAsset;
            renderingData.antialiasingSettings = rpAsset.antialiasingSettings;
            renderingData.shadowSettings = rpAsset.shadowSettings;

            renderingData.cameraData.camera = camera;
            renderingData.cameraData.width = camera.pixelWidth;
            renderingData.cameraData.height = camera.pixelHeight;
            renderingData.cameraData.additionalCameraData = additionalCameraData;
            
            renderingData.renderer = this;
            
            renderingData.cullingResults = context.Cull(ref cullingParameters);
            
            renderingData.runtimeData.isCastShadows = false;
            renderingData.runtimeData.isDeferred = false;
            renderingData.runtimeData.isCameraStackFinally = isCameraStackFinally;
            renderingData.runtimeData.isPost =
                renderingData.cameraData.additionalCameraData.enablePostProcessing;
            renderingData.runtimeData.isAntialiasing = 
                renderingData.cameraData.additionalCameraData.enableAntialiasing && 
                renderingData.antialiasingSettings.antialiasingMode != AntialiasingMode.None;
        }
    }
}