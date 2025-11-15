using System;
using RoXamiRP;
using UnityEngine;
using UnityEngine.Rendering;

public class DecalFeature : RoXamiRenderFeature
{
    DecalPass decalPass;

    public override void Create()
    {
        decalPass = new DecalPass(RenderPassEvent.AfterRenderingGbuffer);
    }

    public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (!IsGameOrSceneCamera(renderingData.cameraData.camera)) return;
#endif

        if (decalPass != null)
        {
            renderer.EnqueuePass(decalPass);
        }
    }
    
    //================================================================================//
    //////////////////////////////////  DecalPass   /////////////////////////////////////
    //================================================================================//
    class DecalPass : RoXamiRenderPass
    {
        public DecalPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        const string bufferName = "RoXamiRP Draw Decal";
        private CommandBuffer cmd;
        RenderingData renderingData;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            this.renderingData = renderingData;
            cmd = renderingData.commandBuffer;
            
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                RoXamiRPCopyTexture(cmd,
                    renderingData.renderer.GetCameraDepthBufferRT(), 
                    renderingData.renderer.GetCameraDepthCopyRT(),
                    ShaderDataID.cameraDepthCopyTextureID);
                
                ExecuteCommandBuffer(context, cmd);
                
                DrawDecal(context);
            }
            
            ExecuteCommandBuffer(context, cmd);
        }

        void DrawDecal(ScriptableRenderContext context)
        {
            // Use the same render targets that GBufferPass already set up
            // Just draw decal objects using context.DrawRenderers

            var sortingSettings = new SortingSettings(renderingData.cameraData.camera)
            {
                criteria = SortingCriteria.CommonTransparent
            };

            var drawingSettings = new DrawingSettings(ShaderDataID.toonGBufferShaderTagId, sortingSettings)
            {
                enableDynamicBatching = renderingData.commonSettings.enableDynamicBatching,
                enableInstancing = renderingData.commonSettings.enableGpuInstancing,
            };

            // Filter for decal objects - you might want to add a specific tag or layer for decals
            var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
            
            // Draw decal objects
            context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
        }
    }
}
