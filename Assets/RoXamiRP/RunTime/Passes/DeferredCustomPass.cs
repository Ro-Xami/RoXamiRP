using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class DeferredCustomPass : RoXamiRenderPass
    {
        readonly m_DeferredLitSettings[] settings;
        private class m_DeferredLitSettings
        {
            public Material mat;
            public int passIndex;
            // public int stencilPropertyID;
            // public int stencil;
        }
        
        public DeferredCustomPass(RenderPassEvent evt, DeferredLitSettings[] deferredLitSettings)
        {
            renderPassEvent = evt;

            if (deferredLitSettings == null || deferredLitSettings.Length == 0)
            {
                return;
            }
            
            settings = new m_DeferredLitSettings[deferredLitSettings.Length];
            for (int i = 0; i < deferredLitSettings.Length; i++)
            {
                settings[i] = new m_DeferredLitSettings
                {
                    mat = deferredLitSettings[i].mat,
                    passIndex = deferredLitSettings[i].passIndex,
                    // stencilPropertyID = Shader.PropertyToID(deferredLitSettings[i].stencilPropertyName),
                    // stencil = deferredLitSettings[i].stencil,
                };
            }
        }

        const string bufferName = "RoXami Custom Toon Deferred";
        private readonly CommandBuffer cmd = new CommandBuffer()
        {
            name = bufferName
        };

        private ScriptableRenderContext context;

        public override void SetUp(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //SetDeferredStencilData();
        }

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderingData)
        {
            context = scriptableRenderContext;

            cmd.BeginSample(bufferName);
            ExecuteCommandBuffer(context, cmd);

            //SetClearRenderTarget();
            Draw();

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {

        }

        void Draw()
        {
            if (settings == null || settings.Length == 0)
            {
                return;
            }

            foreach (var setting in settings)
            {
                if (setting == null || setting.mat == null)
                {
                    continue;
                }
                
                cmd.DrawProcedural(
                    Matrix4x4.identity, setting.mat, setting.passIndex,
                    MeshTopology.Triangles, 3
                );
            }
        }

        // void SetDeferredStencilData()
        // {
        //     if (settings == null || settings.Length == 0)
        //     {
        //         return;
        //     }
        //     
        //     foreach (var setting in settings)
        //     {
        //         if (setting == null)
        //         {
        //             continue;
        //         }
        //         cmd.SetGlobalInt(setting.stencilPropertyID, setting.stencil);
        //     }
        // }
        
        // void SetClearRenderTarget()
        // {
        //     cmd.SetRenderTarget(
        //         ShaderDataID.cameraColorAttachmentId,
        //         RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
        //         ShaderDataID.cameraDepthAttachmentId,
        //         RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        //     cmd.ClearRenderTarget(false, true, Color.clear);
        // }
    }
}