using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class DeferredToonLitFeature : RoXamiRenderFeature
    {
        [SerializeField]
        DeferredLitSettings[] deferredToonLitSettings;
        
        [Serializable]
        class DeferredLitSettings
        {
            public Material mat;
            public int passIndex;
            public string stencilPropertyName;
            public int stencil;
        }
        
        m_DeferredToonPass deferredToonPass;
        
        public override void Create()
        {
            if (deferredToonLitSettings == null || deferredToonLitSettings.Length == 0)
            {
                return;
            }
            
            deferredToonPass = new m_DeferredToonPass(
                RenderPassEvent.BeforeRenderingDeferredLights + 11, 
                deferredToonLitSettings);
        }

        public override void AddRenderPasses(RoXamiRenderer renderer, ref RenderingData renderingData)
        {
            if (deferredToonLitSettings == null || deferredToonLitSettings.Length == 0 ||
                !renderingData.rendererAsset.rendererSettings.enableDeferredRendering)
            {
                return;
            }
            
            renderer.EnqueuePass(deferredToonPass);
        }

        #region Pass
        private class m_DeferredToonPass : RoXamiRenderPass
        {
            readonly m_DeferredLitSettings[] settings;
            private class m_DeferredLitSettings
            {
                public Material mat;
                public int passIndex;
                public int stencilPropertyID;
                public int stencil;
            }
            
            public m_DeferredToonPass(RenderPassEvent evt, DeferredLitSettings[] deferredLitSettings)
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
                        stencilPropertyID = Shader.PropertyToID(deferredLitSettings[i].stencilPropertyName),
                        stencil = deferredLitSettings[i].stencil,
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
                SetDeferredStencilData();
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

            void SetDeferredStencilData()
            {
                if (settings == null || settings.Length == 0)
                {
                    return;
                }
                
                foreach (var setting in settings)
                {
                    if (setting == null)
                    {
                        continue;
                    }
                    cmd.SetGlobalInt(setting.stencilPropertyID, setting.stencil);
                }
            }
            
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
        #endregion
    }
}