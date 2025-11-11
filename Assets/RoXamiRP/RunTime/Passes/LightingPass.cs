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
        const string bufferName = "RoXami Lighting";
        
        public LightingPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        static readonly int directionalLightColorID = Shader.PropertyToID("_DirectionalLightColor");
        static readonly int directionalLightDirectionID = Shader.PropertyToID("_DirectionalLightDirection");
        static readonly int additionalLightCountID = Shader.PropertyToID("_AdditionalLightCount");
        static readonly int additionalLightPositionsID = Shader.PropertyToID("_AdditionalLightPosition");
        static readonly int additionalLightColorsID = Shader.PropertyToID("_AdditionalLightColor");
        static readonly int additionalLightDirectionsID = Shader.PropertyToID("_AdditionalLightDirection");
        static readonly int additionalLightAnglesID = Shader.PropertyToID("_AdditionalLightAngles");

        const int maxDirectionalLightCount = 1;
        const int maxAdditionalLightCount = 64;

        readonly List<Vector4> directionalLightColors = new List<Vector4>(maxDirectionalLightCount);
        readonly List<Vector4> directionalLightDirections = new List<Vector4>(maxAdditionalLightCount);

        readonly List<Vector4> additionalLightColors = new List<Vector4>(maxAdditionalLightCount);
        readonly List<Vector4> additionalLightDirections = new List<Vector4>(maxAdditionalLightCount);
        readonly List<Vector4> additionalLightPositions = new List<Vector4>(maxAdditionalLightCount);
        readonly List<Vector4> additionalLightAngles = new List<Vector4>(maxAdditionalLightCount);

        private CommandBuffer cmd;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd = renderingData.commandBuffer;
            
            SetuplLightPass(ref renderingData, renderingData.cullingResults);
            
            ExecuteCommandBuffer(context, cmd);
        }

        public override void CleanUp(CommandBuffer commandBuffer)
        {
            
        }

        void SetuplLightPass(ref RenderingData renderingData, CullingResults cullingResults)
        {
            ResetLightData(ref renderingData);

            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
            for (int i = 0; i < visibleLights.Length; i++)
            {
                if (directionalLightColors.Count >= maxDirectionalLightCount || 
                    additionalLightColors.Count >= maxAdditionalLightCount)
                {
                    break;
                }
                
                VisibleLight visibleLight = visibleLights[i];
                if (!visibleLight.light) continue;

                switch (visibleLight.lightType)
                {
                    case LightType.Directional:
                        if (directionalLightColors.Count < maxDirectionalLightCount)
                        {
                            SetUpDirectionalLightAndShadow(ref renderingData, i, visibleLight);
                        }
                        break;

                    case LightType.Point:
                        if (additionalLightColors.Count < maxAdditionalLightCount)
                        {
                            SetupPointLight(ref renderingData, i, visibleLight);
                        }
                        break;

                    case LightType.Spot:
                        if (additionalLightColors.Count < maxAdditionalLightCount)
                        {
                            SetupSpotLight(ref renderingData, i, visibleLight);
                        }
                        break;
                }
            }

            SetShaderData();
        }

        private void SetShaderData()
        {
            var directionalLightColor = Vector4.zero;
            var directionalLightDirection = Vector4.zero;
            if (directionalLightColors.Count > 0)
            {
                directionalLightColor = directionalLightColors[0];
                directionalLightDirection = directionalLightDirections[0];
            }
            cmd.SetGlobalVector(directionalLightColorID,  directionalLightColor);
            cmd.SetGlobalVector(directionalLightDirectionID, directionalLightDirection);

            cmd.SetGlobalInt(additionalLightCountID, additionalLightColors.Count);
            if (additionalLightColors.Count > 0)
            {
                cmd.SetGlobalVectorArray(additionalLightColorsID, additionalLightColors.ToArray());
                cmd.SetGlobalVectorArray(additionalLightPositionsID, additionalLightPositions.ToArray());
                cmd.SetGlobalVectorArray(additionalLightDirectionsID, additionalLightDirections.ToArray());
                cmd.SetGlobalVectorArray(additionalLightAnglesID, additionalLightAngles.ToArray());
            }
        }

        private void ResetLightData(ref RenderingData renderingData)
        {
            directionalLightColors.Clear();
            directionalLightDirections.Clear();
            additionalLightColors.Clear();
            additionalLightDirections.Clear();
            additionalLightPositions.Clear();
            additionalLightAngles.Clear();
        }

        //Set Typed Light Data
        void SetUpDirectionalLightAndShadow(ref RenderingData renderingData, int lightIndex, VisibleLight visibleLight)
        {
            directionalLightColors.Add(visibleLight.finalColor);
            directionalLightDirections.Add(-visibleLight.localToWorldMatrix.GetColumn(2));
            
            renderingData.lightData.directionalLights.Add(visibleLight.light);
            AddShadowCasterLight(renderingData, lightIndex, visibleLight);
        }

        void SetupPointLight(ref RenderingData renderingData, int lightIndex, VisibleLight visibleLight)
        {
            Vector4 position = GetAdditionalLightAttenuation(visibleLight);

            additionalLightColors.Add(visibleLight.finalColor);
            additionalLightPositions.Add(position);
            additionalLightAngles.Add(new Vector4(0f, 1f));
            //AddShadowCasterLight(renderingData, lightIndex, visibleLight);
        }

        void SetupSpotLight(ref RenderingData renderingData, int lightIndex, VisibleLight visibleLight)
        {
            Vector4 position = GetAdditionalLightAttenuation(visibleLight);
            
            Light light = visibleLight.light;
            float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
            float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
            Vector4 angle = new Vector4(angleRangeInv, -outerCos * angleRangeInv);

            additionalLightColors.Add(visibleLight.finalColor);
            additionalLightPositions.Add(position);
            additionalLightDirections.Add(-visibleLight.localToWorldMatrix.GetColumn(2));
            additionalLightAngles.Add(angle);
            
            //AddShadowCasterLight(renderingData, lightIndex, visibleLight);
        }
        
        
        private void AddShadowCasterLight(RenderingData renderingData, int lightIndex, VisibleLight visibleLight)
        {
            if (visibleLight.light.shadows != LightShadows.None && visibleLight.light.shadowStrength > 0)
            {
                var shadowCasterLight = new ShadowCasterLight();
                shadowCasterLight.light = visibleLight.light;
                shadowCasterLight.lightIndex = lightIndex;
                renderingData.lightData.shadowCasterLights.Add(shadowCasterLight);
            }
        }

        Vector4 GetAdditionalLightAttenuation(VisibleLight visibleLight)
        {
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(0.0001f, visibleLight.range * visibleLight.range);
            return position;
        }
    }
}