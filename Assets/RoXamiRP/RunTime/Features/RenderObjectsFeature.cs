using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class RenderObjectsFeature : RoXamiRenderFeature
    {
        [SerializeField]
        RenderObjectSettings settings = new RenderObjectSettings();
        
        [Serializable]
        class RenderObjectSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            public SortingCriteria sortingSettings = SortingCriteria.CommonOpaque;
            public LayerMask layerMask;
            public string[] lightModes;
            [SerializeField]
            public ParamSettings paramSettings = new ParamSettings();
        }

        [Serializable]
        public class ParamSettings
        {
            [SerializeField]
            public FloatParam[] floatParams;
            [SerializeField]
            public VectorParam[] vectorParams;
            [SerializeField]
            public ColorParam[] colorParams;
        }

        [Serializable]
        public class FloatParam
        {
            public float floatParam;
            public string floatParamID;
        }

        [Serializable]
        public class VectorParam
        {
            public Vector4 vectorParam;
            public string vectorParamID;
        }

        [Serializable]
        public class ColorParam
        {
            public Color colorParam;
            public string colorParamID;
        }

        private RenderObjectsPass pass;
        
        public override void Create()
        {
            if (settings.renderPassEvent <= RenderPassEvent.AfterRenderingPrePasses)
            {
                settings.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            }

            if (settings.renderPassEvent >= RenderPassEvent.BeforeRenderingPostProcessing)
            {
                settings.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            }
            
            var paramSettings = settings.paramSettings;
            if (paramSettings != null)
            {
                if (paramSettings.floatParams == null)
                {
                    return;
                }
                foreach (var p in paramSettings.floatParams)
                {
                    if (p.floatParamID != null)
                    {
                        Shader.SetGlobalFloat(p.floatParamID, p.floatParam);
                    }
                }
                
                if (paramSettings.vectorParams == null)
                {
                    return;
                }
                foreach (var p in paramSettings.vectorParams)
                {
                    if (p.vectorParamID != null)
                    {
                        Shader.SetGlobalVector(p.vectorParamID, p.vectorParam);
                    }
                }
                
                if (paramSettings.colorParams == null)
                {
                    return;
                }
                foreach (var p in paramSettings.colorParams)
                {
                    if (p.colorParamID != null)
                    {
                        Shader.SetGlobalColor(p.colorParamID, p.colorParam);
                    }
                }
            }
            
            pass = new RenderObjectsPass(settings);
        }

        public override void AddRenderPasses(RoXamiRenderLoop renderLoop, ref RenderingData renderingData)
        {
            if (pass != null)
            {
                renderLoop.EnqueuePass(pass);
            }
            
        }

        private class RenderObjectsPass : RoXamiRenderPass
        {
            DrawingSettings drawingSettings = new DrawingSettings();
            FilteringSettings filteringSettings = new FilteringSettings();
            private readonly RenderObjectSettings settings;
            private List<ShaderTagId> shaderTagID = new List<ShaderTagId>();
            public RenderObjectsPass(RenderObjectSettings settings)
            {
                if (settings == null || settings.lightModes == null || settings.lightModes.Length == 0)
                {
                    return;
                }
                
                renderPassEvent = settings.renderPassEvent;
                this.settings = settings;

                SortingSettings sortingSettings = new SortingSettings(/*renderingData.cameraData.camera*/)
                {
                    criteria = settings.sortingSettings
                };

                drawingSettings = new DrawingSettings()
                {
                    //enableDynamicBatching = renderingData.commonSettings.enableDynamicBatching,
                    enableInstancing = true,
                    perObjectData = PerObjectData.ReflectionProbes | PerObjectData.LightProbe,
                    sortingSettings = sortingSettings
                };

                for (int i = 0; i < settings.lightModes.Length; i++)
                {
                    var lightMode = settings.lightModes[i];
                    drawingSettings.SetShaderPassName(i, new ShaderTagId(lightMode));
                }
                filteringSettings = new FilteringSettings(RenderQueueRange.all);
                filteringSettings.layerMask = settings.layerMask;
            }
            
            const string bufferName = "RoXamiRP RenderObjects";
            CommandBuffer cmd = new CommandBuffer()
            {
                name = bufferName,
            };
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                cmd.BeginSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
                
                context.DrawRenderers(renderingData.cullingResults, ref drawingSettings, ref filteringSettings);
                
                cmd.EndSample(bufferName);
                ExecuteCommandBuffer(context, cmd);
            }
        }
    }
}