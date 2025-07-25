using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    public class Shadows
    {
        const string bufferName = "RoXami Shadows";

        CullingResults cullingResults;
        ScriptableRenderContext context;
        ShadowSettings settings;

        private static readonly CommandBuffer cmd = new CommandBuffer
        {
            name = bufferName
        };

        private const int maxCascades = 4;

        private static readonly int directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
        Vector4 directionalLightShadowData;
        private readonly Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
        private readonly Matrix4x4[] directionalShadowMatrices = new Matrix4x4[maxCascades];
        Vector4 shadowDistanceFade;

        private static ScreenSpaceShadowsData screenSpaceShadowsData = new ScreenSpaceShadowsData();

        public void Setup(RenderingData renderingData)
        {
            this.cullingResults = renderingData.cullingResults;
            this.context = renderingData.context;
            this.settings = renderingData.shadowSettings;
        }

        public void Render(Light light, int lightIndex)
        {
            if (light != null
                && light.shadows != LightShadows.None && light.shadowStrength > 0f &&
                cullingResults.GetShadowCasterBounds(0, out Bounds b))
            {
                RenderDirectionalShadows(light, lightIndex);
            }
            else
            {
                directionalLightShadowData = Vector4.zero;

                cmd.GetTemporaryRT(
                    directionalShadowAtlasID, 1, 1,
                    32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
                );
            }
        }

        public void GetScreenSpaceShadowsData(out ScreenSpaceShadowsData ssShadowsData)
        {
            ssShadowsData = screenSpaceShadowsData;
        }

        void RenderDirectionalShadows(Light light, int lightIndex)
        {

            int atlasSize = (int)settings.directional.atlasSize;

            cmd.GetTemporaryRT(directionalShadowAtlasID, atlasSize, atlasSize,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap); //use unity's textureFormat for different API

            cmd.SetRenderTarget(directionalShadowAtlasID,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            cmd.ClearRenderTarget(true, false, Color.clear);

            cmd.BeginSample(bufferName);
            ExecuteBuffer();

            int split = 2; //only 4 cascades
            int tileSize = atlasSize / split; //get 1 cascade texture size

            RenderDirectionalShadows(split, tileSize, light, lightIndex);

            cmd.EndSample(bufferName);
            ExecuteBuffer();
        }

        void RenderDirectionalShadows(int split, int tileSize, Light light, int lightIndex)
        {

            //directional light's camera is orthographic
            ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(
                cullingResults, lightIndex); //, BatchCullingProjectionType.Orthographic);

            //only 4 cascades, get cascade data
            int cascadeCount = maxCascades;
            Vector3 ratios = settings.directional.CascadeRatios;

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
                Vector2 atlasOffset = new Vector2(i % split, (float)i / split);
                cmd.SetViewport(new Rect(atlasOffset.x * tileSize, atlasOffset.y * tileSize, tileSize, tileSize));

                Matrix4x4 matrix = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, atlasOffset, split);
                directionalShadowMatrices[i] = matrix;

                cmd.SetGlobalDepthBias(0f, light.shadowBias);
                ExecuteBuffer();
                context.DrawShadows(ref shadowSettings);
            }

            //set shader data
            int atlasSize = (int)settings.directional.atlasSize;
            directionalLightShadowData =
                new Vector4(light.shadowStrength, light.shadowNormalBias, cascadeCount, atlasSize);
            shadowDistanceFade =
                new Vector4(settings.maxDistance, settings.distanceFade, settings.directional.cascadeFade);

            screenSpaceShadowsData.directionalShadowAtlasID = directionalShadowAtlasID;
            screenSpaceShadowsData.directionalLightShadowData = directionalLightShadowData;
            screenSpaceShadowsData.cascadeCullingSpheres = cascadeCullingSpheres;
            screenSpaceShadowsData.directionalShadowMatrices = directionalShadowMatrices;
            screenSpaceShadowsData.shadowDistanceFade = shadowDistanceFade;
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

        public void CleanUp()
        {
            cmd.ReleaseTemporaryRT(directionalShadowAtlasID);
            ExecuteBuffer();
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}
