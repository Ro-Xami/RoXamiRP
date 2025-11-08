using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using RoXamiRP;

namespace RoXamiRP
{
    public class LightingPass : RoXamiRenderPass
    {
        public LightingPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        const int maxDirectionalLightCount = 1;
        const int maxAdditionalLightCount = 64;

        const string bufferName = "RoXami Lighting";
        CullingResults cullingResults;
        ScriptableRenderContext context;

        static readonly Shadows shadows = new Shadows();

        static readonly int
            dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
            dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection"),
            addLightCountID = Shader.PropertyToID("_AdditionalLightCount"),
            addLightPositionID = Shader.PropertyToID("_AdditionalLightPosition"),
            addLightColorID = Shader.PropertyToID("_AdditionalLightColor"),
            addLightDirectionID = Shader.PropertyToID("_AdditionalLightDirection"),
            addLightAnglesID = Shader.PropertyToID("_AdditionalLightAngles");

        static Vector4
            dirLightColor, dirLightDirection;

        static Vector4[]
            addLightPosition = new Vector4[maxAdditionalLightCount],
            addLightColor = new Vector4[maxAdditionalLightCount],
            addLightDirection = new Vector4[maxAdditionalLightCount],
            addLightAngles = new Vector4[maxAdditionalLightCount];

        readonly CommandBuffer cmd = new CommandBuffer
        {
            name = bufferName
        };

        public override void Execute(ScriptableRenderContext scriptableRenderContext, ref RenderingData renderingData)
        {
            cullingResults = renderingData.cullingResults;
            context = scriptableRenderContext;

            cmd.BeginSample(bufferName);

            shadows.Setup(context, renderingData);

            SetupDirectionalLight(ref renderingData);

            cmd.EndSample(bufferName);
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp()
        {
            shadows.CleanUp();
        }

        void SetupDirectionalLight(ref RenderingData renderingData)
        {
            int addLightCount = 0;
            int dirLightCount = 0;
            int dirLightIndex = 0;

            Light dirLight = null;
            dirLightColor = Vector4.zero;
            dirLightDirection = Vector4.zero;
            addLightPosition = new Vector4[maxAdditionalLightCount];
            addLightColor = new Vector4[maxAdditionalLightCount];
            addLightDirection = new Vector4[maxAdditionalLightCount];
            addLightAngles = new Vector4[maxAdditionalLightCount];

            renderingData.lightData.directionalLights = new List<Light>();
            renderingData.lightData.additionalLights = new List<Light>();
            renderingData.lightData.pointLights = new List<Light>();
            renderingData.lightData.spotLights = new List<Light>();

            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
            for (int i = 0; i < visibleLights.Length; i++)
            {
                VisibleLight visibleLight = visibleLights[i];
                switch (visibleLight.lightType)
                {
                    case LightType.Directional:
                        if (dirLightCount < maxDirectionalLightCount)
                        {
                            SetUpDirectionalLightAndShadow(visibleLight);
                            dirLight = visibleLight.light;
                            dirLightCount++;
                            
                            renderingData.lightData.directionalLights.Add(dirLight);
                        }

                        break;

                    case LightType.Point:
                        if (addLightCount < maxAdditionalLightCount)
                        {
                            SetupPointLight(addLightCount, ref visibleLight);
                            addLightCount++;
                            
                            renderingData.lightData.pointLights.Add(visibleLight.light);
                            renderingData.lightData.additionalLights.Add(visibleLight.light);
                        }

                        break;

                    case LightType.Spot:
                        if (addLightCount < maxAdditionalLightCount)
                        {
                            SetupSpotLight(addLightCount, ref visibleLight);
                            addLightCount++;
                            
                            renderingData.lightData.spotLights.Add(visibleLight.light);
                            renderingData.lightData.additionalLights.Add(visibleLight.light);
                        }

                        break;
                }
            }

            shadows.Render(dirLight, dirLightIndex, ref renderingData);
            cmd.SetGlobalVector(dirLightColorId, dirLightColor);
            cmd.SetGlobalVector(dirLightDirectionId, dirLightDirection);

            cmd.SetGlobalInt(addLightCountID, addLightCount);
            cmd.SetGlobalVectorArray(addLightColorID, addLightColor);
            cmd.SetGlobalVectorArray(addLightPositionID, addLightPosition);
            cmd.SetGlobalVectorArray(addLightAnglesID, addLightAngles);
            cmd.SetGlobalVectorArray(addLightDirectionID, addLightDirection);
        }

        void SetUpDirectionalLightAndShadow(VisibleLight visibleLight)
        {
            dirLightColor = visibleLight.finalColor;
            dirLightDirection = -visibleLight.localToWorldMatrix.GetColumn(2);
        }

        void SetupPointLight(int lightIndex, ref VisibleLight visibleLight)
        {
            Vector4 position = GetAdditionalLightAttenuation(ref visibleLight);

            addLightColor[lightIndex] = visibleLight.finalColor;
            addLightPosition[lightIndex] = position;
            addLightAngles[lightIndex] = new Vector4(0f, 1f);
        }

        void SetupSpotLight(int lightIndex, ref VisibleLight visibleLight)
        {
            Vector4 position = GetAdditionalLightAttenuation(ref visibleLight);

            addLightColor[lightIndex] = visibleLight.finalColor;
            addLightPosition[lightIndex] = position;
            addLightDirection[lightIndex] = -visibleLight.localToWorldMatrix.GetColumn(2);

            Light light = visibleLight.light;
            float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
            float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
            addLightAngles[lightIndex] = new Vector4(
                angleRangeInv, -outerCos * angleRangeInv
            );
        }

        Vector4 GetAdditionalLightAttenuation(ref VisibleLight visibleLight)
        {
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(0.0001f, visibleLight.range * visibleLight.range);
            return position;
        }
    }
}