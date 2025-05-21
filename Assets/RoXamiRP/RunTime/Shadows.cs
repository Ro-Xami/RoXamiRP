using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    const string bufferName = "ToonShadows";

    CullingResults cullingResults;
    ScriptableRenderContext context;
    ShadowSettings settings;

    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };

    const int maxCascades = 4;

    static string[] directionalFilterKeywords = {
        "_DIRECTIONAL_PCF3",
        "_DIRECTIONAL_PCF5",
        "_DIRECTIONAL_PCF7",
    };

    int
        directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"),
        directionalShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices"),
        cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
        shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
        //shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");

    Vector4 dirLightShadowData;
    Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
    Matrix4x4[] directionalShadowMatrices = new Matrix4x4[maxCascades];
    Vector4 shadowDistanceFade;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        this.context = context;
        this.settings = shadowSettings;
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
            cmd.SetGlobalVector(dirLightShadowDataId, Vector4.zero);

            cmd.GetTemporaryRT(
                directionalShadowAtlasID, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
        }
    }

    void RenderDirectionalShadows(Light light , int lightIndex) {

        int atlasSize = (int)settings.directional.atlasSize;

        cmd.GetTemporaryRT(directionalShadowAtlasID, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);//尽可能高的深度位数，~，取决于目标平台的贴图格式

        //从GPU渲染纹理而不是从摄像机渲染纹理
        cmd.SetRenderTarget(directionalShadowAtlasID,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

        cmd.ClearRenderTarget(true, false, Color.clear);

        cmd.BeginSample(bufferName);
        ExecuteBuffer();

        int split = 2;//仅四级级联
        int tileSize = atlasSize / split;//单个级联的大小

        RenderDirectionalShadows(split , tileSize , light, lightIndex);

        cmd.EndSample(bufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows(int split, int tileSize , Light light , int lightIndex) {

        //阴影绘制设置
        ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(
            cullingResults, lightIndex, BatchCullingProjectionType.Orthographic);

        //获取级联设置
        int cascadeCount = maxCascades;
        Vector3 ratios = settings.directional.CascadeRatios;

        //为每个级联渲染图集
        for ( int i = 0; i < cascadeCount; i++ )
        {
            //输出光源视口矩阵，光源投影矩阵，级联数据
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            lightIndex, i, cascadeCount, ratios, tileSize, light.shadowNearPlane,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

            shadowSettings.splitData = splitData;
            //级联包围球
            Vector4 cullingSphere = splitData.cullingSphere;//xyz包围球中心，w包围球半径
            cullingSphere.w *= cullingSphere.w;//提前计算包围球的平方距离，减少shader计算
            cascadeCullingSpheres[i] = cullingSphere;

            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            //计算图集在每个级联的偏移值
            Vector2 atlasOffset = new Vector2(i % split, i / split);
            //设置渲染视口
            cmd.SetViewport(new Rect(atlasOffset.x * tileSize, atlasOffset.y * tileSize, tileSize, tileSize));

            //矩阵计算
            Matrix4x4 matrix = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, atlasOffset, split);
            directionalShadowMatrices[i] = matrix;

            cmd.SetGlobalDepthBias(0f, light.shadowBias);//深度偏移
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
        }

        //传入参数
        int atlasSize = (int)settings.directional.atlasSize;
        dirLightShadowData = new Vector4(light.shadowStrength, light.shadowNormalBias, cascadeCount , atlasSize);
        cmd.SetGlobalVector(dirLightShadowDataId, dirLightShadowData);

        cmd.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
        cmd.SetGlobalMatrixArray(directionalShadowMatricesID, directionalShadowMatrices);

        shadowDistanceFade = new Vector4(settings.maxDistance, settings.distanceFade, settings.directional.cascadeFade);
        cmd.SetGlobalVector(shadowDistanceFadeId, shadowDistanceFade);

        SetKeywords();
    }

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        //unity在某些平台上使用反向ZBuffer
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }

        float scale = 1f / split;
        //-1~1转化到0~1
        m.m00 = 0.5f * (m.m00) * scale;
        m.m01 = 0.5f * (m.m01) * scale;
        m.m02 = 0.5f * (m.m02) * scale;
        m.m03 = (0.5f * (m.m03 + 1) + offset.x) * scale;//计算图集的偏移
        m.m10 = 0.5f * (m.m10) * scale;
        m.m11 = 0.5f * (m.m11) * scale;
        m.m12 = 0.5f * (m.m12) * scale;
        m.m13 = (0.5f * (m.m13 + 1) + offset.y) *scale;//计算图集的偏移
        m.m20 = 0.5f * (m.m20);
        m.m21 = 0.5f * (m.m21);
        m.m22 = 0.5f * (m.m22);
        m.m23 = 0.5f * (m.m23 + 1);
        return m;
    }

    void SetKeywords()
    {
        int enabledIndex = (int)settings.directional.filter - 1;
        for (int i = 0; i < directionalFilterKeywords.Length; i++)
        {
            if (i == enabledIndex)
            {
                cmd.EnableShaderKeyword(directionalFilterKeywords[i]);
            }
            else
            {
                cmd.DisableShaderKeyword(directionalFilterKeywords[i]);
            }
        }
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
