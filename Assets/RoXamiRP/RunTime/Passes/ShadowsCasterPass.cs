using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public class ShadowsCasterPass : RoXamiRenderPass
    {
        const string bufferName = "RoXami ShadowsCaster";
        public ShadowsCasterPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(bufferName);
        }

        private const int maxCascades = 4;
        private const int split = 2;

        private RTHandle directionalLightShadowAtlas;
        RenderTextureDescriptor descriptor = new RenderTextureDescriptor(1, 1)
        {
            depthBufferBits = 32,
            colorFormat = RenderTextureFormat.Shadowmap
        };
        
        private static readonly int directionalLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");
        private static readonly int directionalShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices");
        private static readonly int cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
        private static readonly int shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
        
        Vector4 directionalLightShadowData;
        private readonly Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
        private readonly Matrix4x4[] directionalShadowMatrices = new Matrix4x4[maxCascades];
        Vector4 shadowDistanceFade;

        private CommandBuffer cmd;
        ShadowSettings settings;
        CullingResults cullingResults;
        ScriptableRenderContext context;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //only directionalLight
            bool isDirectionalLight = false;
            foreach (var lightType in renderingData.lightData.shadowCasterLights)
            {
                if (lightType.light.type == LightType.Directional)
                {
                    isDirectionalLight = true;
                }
            }
            if (!isDirectionalLight)
            {
                return;
            }
            
            if (!renderingData.cameraData.additionalCameraData.enableScreenSpaceShadows ||
                !renderingData.shadowSettings.enableDirectionalShadows)
            {
                return;
            }
            
            settings = renderingData.shadowSettings;
            cullingResults = renderingData.cullingResults;
            this.context = context;
            
            cmd = renderingData.commandBuffer;
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                ExecuteCommandBuffer(context, cmd);
                
                foreach (var shadowCasterLight in renderingData.lightData.shadowCasterLights)
                {
                    if (!renderingData.cullingResults.GetShadowCasterBounds(0, out Bounds b))
                    {
                        continue;
                    }
                    renderingData.runtimeData.isCastShadows = true;
                    
                    RenderDirectionalShadows(ref renderingData, shadowCasterLight.light, shadowCasterLight.lightIndex);
                }
            }
            
            ExecuteCommandBuffer(context, cmd);
        }

        public override void Dispose()
        {
            directionalLightShadowAtlas?.Release();
        }

        void RenderDirectionalShadows(ref RenderingData renderingData, Light light, int lightIndex)
        {

            int atlasSize = (int)settings.directional.atlasSize;
            descriptor.width = atlasSize;
            descriptor.height = atlasSize;
            
            RoXamiRTHandlePool.GetRTHandleIfNeeded(ref directionalLightShadowAtlas,
                descriptor, FilterMode.Point, ShaderDataID.directionalShadowAtlasName, TextureWrapMode.Clamp, true);

            cmd.SetRenderTarget(directionalLightShadowAtlas,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            cmd.ClearRenderTarget(true, false, Color.clear);

            ExecuteCommandBuffer(context, cmd);

            RenderDirectionalShadowsCascade(light, lightIndex);
            
            renderingData.cameraData.directionalLightShadowAtlas = directionalLightShadowAtlas;
        }

        void RenderDirectionalShadowsCascade(Light light, int lightIndex)
        {
            //directional light's camera is orthographic
            ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(
                cullingResults, lightIndex, BatchCullingProjectionType.Orthographic);

            //only 4 cascades, get cascade data
            int cascadeCount = maxCascades;
            Vector3 ratios = settings.directional.CascadeRatios;

            int tileSize = (int)settings.directional.atlasSize / split;

            //for different cascades datas
            for (int i = 0; i < cascadeCount; i++)
            {
                //api for draw shadow, get matrix and split data
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    lightIndex, i, cascadeCount, ratios, tileSize, light.shadowNearPlane,
                    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

                shadowSettings.splitData = splitData;
                Vector4 cullingSphere = splitData.cullingSphere; //sphere's center
                cullingSphere.w *= cullingSphere.w; //power of 2 culling sphere's radios
                cascadeCullingSpheres[i] = cullingSphere;

                cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

                //current cascade's offest on the shadowMap
                Vector2 atlasOffset = new Vector2(i % split, (int)(i / split));
                cmd.SetViewport(new Rect(atlasOffset.x * tileSize, atlasOffset.y * tileSize, tileSize, tileSize));

                Matrix4x4 matrix = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, atlasOffset, split);
                directionalShadowMatrices[i] = matrix;

                cmd.SetGlobalDepthBias(0f, light.shadowBias);
                ExecuteCommandBuffer(context, cmd);
                context.DrawShadows(ref shadowSettings);
            }

            //set shader data
            int atlasSize = (int)settings.directional.atlasSize;
            directionalLightShadowData =
                new Vector4(light.shadowStrength, light.shadowNormalBias, cascadeCount, atlasSize);
            shadowDistanceFade =
                new Vector4(settings.maxDistance, settings.distanceFade, settings.directional.cascadeFade);
            cmd.SetGlobalVector(directionalLightShadowDataId, directionalLightShadowData);
            cmd.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
            cmd.SetGlobalMatrixArray(directionalShadowMatricesID, directionalShadowMatrices);
            cmd.SetGlobalVector(shadowDistanceFadeId, shadowDistanceFade);
        }

        Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
        {
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }

            float scale = 1f / split;
            //-1~1 to 0~1
            m.m00 = 0.5f * (m.m00) * scale;
            m.m01 = 0.5f * (m.m01) * scale;
            m.m02 = 0.5f * (m.m02) * scale;
            m.m03 = (0.5f * (m.m03 + 1) + offset.x) * scale; //cascade's shadow map offest
            m.m10 = 0.5f * (m.m10) * scale;
            m.m11 = 0.5f * (m.m11) * scale;
            m.m12 = 0.5f * (m.m12) * scale;
            m.m13 = (0.5f * (m.m13 + 1) + offset.y) * scale; //cascade's shadow map offest
            m.m20 = 0.5f * (m.m20);
            m.m21 = 0.5f * (m.m21);
            m.m22 = 0.5f * (m.m22);
            m.m23 = 0.5f * (m.m23 + 1);
            return m;
        }
    }
}
