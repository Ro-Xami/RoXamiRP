using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public partial class RoXamiRenderer
    {
        Camera camera;
        ScriptableRenderContext context;
        private RenderingData renderingData = new RenderingData();
    
        const string baseBufferName = "RoXamiRP Renderer:";
        private string rendererBufferName;
        private CommandBuffer rendererCommandBuffer;

        public void Render(ScriptableRenderContext context)
        {
            Profiler.BeginSample("RoXamiRP Editor Profiler");
            
            this.context = context;
            camera = renderingData.cameraData.camera;

            RTHandles.ResetReferenceSize(renderingData.cameraData.width, renderingData.cameraData.height);
            SetUpCameraAttachment();

            rendererBufferName = baseBufferName + camera.name;
            rendererCommandBuffer = CommandBufferPool.Get(rendererBufferName);
            using (new ProfilingScope(rendererCommandBuffer, new ProfilingSampler(rendererBufferName)))
            {
                SetCommonData();
                context.ExecuteCommandBuffer(rendererCommandBuffer);
                rendererCommandBuffer.Clear();
  
                DrawCamera(context);

                CommandBufferPool.Release(rendererCommandBuffer);
            }
            context.ExecuteCommandBuffer(rendererCommandBuffer);
            rendererCommandBuffer.Clear();

            context.Submit();
            
            Profiler.EndSample();
        }

        private void DrawCamera(ScriptableRenderContext context)
        {
            var cmd = CommandBufferPool.Get();
            renderingData.commandBuffer = cmd;
            #region DrawEditor
            PrepareForSceneWindow();
            #endregion

            CameraSetup(cmd, ref renderingData);
            ExecuteRoXamiRenderPass(context, ref renderingData);

            #region DrawEditor
            DrawUnsupportedShaders();
            DrawGizmos();
            DrawWire();
            #endregion

            CameraCleanUp(cmd);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            CommandBufferPool.Release(cmd);
        }

        void SetCommonData()
        {
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);

            Matrix4x4 vpMatrix = projMatrix * viewMatrix;
            Matrix4x4 invVP = vpMatrix.inverse;

            rendererCommandBuffer.SetGlobalMatrix(ShaderDataID.matrixInvVP_ID, invVP);
            
            rendererCommandBuffer.DisableShaderKeyword(ShaderDataID.enableScreenSpaceShadowsID);
            rendererCommandBuffer.DisableShaderKeyword(ShaderDataID.enableScreenSpaceReflectionID);
        }

        internal void Dispose()
        {
            foreach (var pass in activePasses)
            {
                pass?.Dispose();
            }
            cameraColorAttachmentART?.Release();
            cameraColorAttachmentBRT?.Release();
            cameraDepthAttachmentRT?.Release();
        }

        internal void InitializedRenderingData( 
            ScriptableRenderContext context, 
            Camera camera, AdditionalCameraData additionalCameraData, 
            RoXamiRPAsset rpAsset, RoXamiRendererAsset rendererAsset, 
            bool isCameraStackFinally, out bool cull)
        {
            if (!camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                cull = false;
                return;
            }
            p.shadowDistance = Mathf.Min(rpAsset.shadowSettings.maxDistance, camera.farClipPlane);
            var cullingResults = context.Cull(ref p);
            cull = true;
            
            renderingData.commonSettings = rpAsset.commonSettings;
            renderingData.shaderAsset = rpAsset.shaderAsset;
            renderingData.antialiasingSettings = rpAsset.antialiasingSettings;
            renderingData.shadowSettings = rpAsset.shadowSettings;
            
            renderingData.lightData.directionalLights = new List<Light>();
            renderingData.lightData.additionalLights = new List<Light>();
            renderingData.lightData.pointLights = new List<Light>();
            renderingData.lightData.spotLights = new List<Light>();
            renderingData.lightData.shadowCasterLights = new List<ShadowCasterLight>();
            
            renderingData.rendererSettings = rendererAsset.rendererSettings;

            renderingData.cameraData.camera = camera;
            renderingData.cameraData.width = camera.pixelWidth;
            renderingData.cameraData.height = camera.pixelHeight;
            renderingData.cameraData.additionalCameraData = additionalCameraData;
            
            renderingData.renderer = this;
            
            renderingData.cullingResults = cullingResults;
            
            renderingData.runtimeData.isCastShadows = false;
            renderingData.runtimeData.isCameraStackFinally = isCameraStackFinally;
            renderingData.runtimeData.isPost =
                renderingData.cameraData.additionalCameraData.enablePostProcessing;
            renderingData.runtimeData.isAntialiasing = 
                renderingData.cameraData.additionalCameraData.enableAntialiasing && 
                renderingData.antialiasingSettings.antialiasingMode != AntialiasingMode.None;
        }
    }
}