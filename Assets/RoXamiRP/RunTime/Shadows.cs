using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    const int lightIndex = 0;

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

    static int directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
    static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");
    static int directionalShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices");
    static int cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
    static int shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

    static Vector3 dirLightShadowData;
    static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
    static Matrix4x4[] directionalShadowMatrices = new Matrix4x4[maxCascades];

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        this.context = context;
        this.settings = shadowSettings;
    }

    public void Render(Light light)
    {
        if (light != null 
            && light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            cullingResults.GetShadowCasterBounds(0, out Bounds b))
        {
            RenderDirectionalShadows(light);
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

    void RenderDirectionalShadows(Light light) {

        int atlasSize = (int)settings.directional.atlasSize;

        cmd.GetTemporaryRT(directionalShadowAtlasID, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);//�����ܸߵ����λ����~��ȡ����Ŀ��ƽ̨����ͼ��ʽ

        //��GPU��Ⱦ��������Ǵ��������Ⱦ����
        cmd.SetRenderTarget(directionalShadowAtlasID,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

        cmd.ClearRenderTarget(true, false, Color.clear);

        cmd.BeginSample(bufferName);
        ExecuteBuffer();

        int split = settings.directional.cascadeCount > 1 ? 2 : 1;//���ݼ�����������ƽ������
        int tileSize = atlasSize / split;//���������Ĵ�С

        RenderDirectionalShadows(split , tileSize , light);

        cmd.EndSample(bufferName);
    }

    void RenderDirectionalShadows(int split, int tileSize , Light light) {

        //��Ӱ��������
        ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(
            cullingResults, lightIndex, BatchCullingProjectionType.Orthographic);

        //��ȡ��������
        int cascadeCount = settings.directional.cascadeCount;
        Vector3 ratios = settings.directional.CascadeRatios;

        //Ϊÿ��������Ⱦͼ��
        for ( int i = 0; i < cascadeCount; i++ )
        {
            //�����Դ�ӿھ��󣬹�ԴͶӰ���󣬼�������
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            lightIndex, i, cascadeCount, ratios, tileSize, light.shadowNearPlane,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

            shadowSettings.splitData = splitData;
            //������Χ��
            Vector4 cullingSphere = splitData.cullingSphere;//xyz��Χ�����ģ�w��Χ��뾶
            cullingSphere.w *= cullingSphere.w;//��ǰ�����Χ���ƽ�����룬����shader����
            cascadeCullingSpheres[i] = cullingSphere;

            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            //����ͼ����ÿ��������ƫ��ֵ
            Vector2 atlasOffset = new Vector2(i % split, i / split);
            //������Ⱦ�ӿ�
            cmd.SetViewport(new Rect(atlasOffset.x * tileSize, atlasOffset.y * tileSize, tileSize, tileSize));

            //�������
            Matrix4x4 matrix = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, atlasOffset, split);
            directionalShadowMatrices[i] = matrix;

            cmd.SetGlobalDepthBias(0f, light.shadowBias);//���ƫ��
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
        }

        dirLightShadowData = new Vector3(light.shadowStrength, light.shadowNormalBias, cascadeCount);
        //�������
        cmd.SetGlobalVector(dirLightShadowDataId, dirLightShadowData);
        cmd.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
        cmd.SetGlobalMatrixArray(directionalShadowMatricesID, directionalShadowMatrices);
        cmd.SetGlobalVector(shadowDistanceFadeId,new Vector4(1f / settings.maxDistance, 1f / settings.distanceFade));
    }

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        //unity��ĳЩƽ̨��ʹ�÷���ZBuffer
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }

        float scale = 1f / split;
        //-1~1ת����0~1
        m.m00 = 0.5f * (m.m00) * scale;
        m.m01 = 0.5f * (m.m01) * scale;
        m.m02 = 0.5f * (m.m02) * scale;
        m.m03 = (0.5f * (m.m03 + 1) + offset.x) * scale;//����ͼ����ƫ��
        m.m10 = 0.5f * (m.m10) * scale;
        m.m11 = 0.5f * (m.m11) * scale;
        m.m12 = 0.5f * (m.m12) * scale;
        m.m13 = (0.5f * (m.m13 + 1) + offset.y) *scale;//����ͼ����ƫ��
        m.m20 = 0.5f * (m.m20);
        m.m21 = 0.5f * (m.m21);
        m.m22 = 0.5f * (m.m22);
        m.m23 = 0.5f * (m.m23 + 1);
        return m;

        //if (SystemInfo.usesReversedZBuffer)
        //{
        //    m.m20 = -m.m20;
        //    m.m21 = -m.m21;
        //    m.m22 = -m.m22;
        //    m.m23 = -m.m23;
        //}
        //float scale = 1f / split;
        //m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        //m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        //m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        //m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        //m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        //m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        //m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        //m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        //m.m20 = 0.5f * (m.m20 + m.m30);
        //m.m21 = 0.5f * (m.m21 + m.m31);
        //m.m22 = 0.5f * (m.m22 + m.m32);
        //m.m23 = 0.5f * (m.m23 + m.m33);
        //return m;
    }

    /// <summary>
    /// �ͷ�Shadow Map
    /// </summary>
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

    //���Լ�����Χ��
#if UNITY_EDITOR
    public void DrawCascadesGizmos(Camera camera)
    {
        if (settings == null || camera == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < settings.directional.cascadeCount; i++)
        {
            Vector4 sphere = cascadeCullingSpheres[i];
            Vector3 center = new Vector3(sphere.x, sphere.y, sphere.z);
            float radius = Mathf.Sqrt(sphere.w);

            // Transform������ռ䣨�������Ҫ��
            Gizmos.DrawWireSphere(center, radius);
        }
    }
#endif
}
