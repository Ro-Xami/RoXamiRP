using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    const string bufferName = "ToonShadows";
    
    CullingResults cullingResults;
    ScriptableRenderContext context;
    ShadowSettings shadowSettings;

    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };

    const int maxShadowDirectionalLightCount = 2;
    int shadowedDirectionalLightCount;
    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }
    ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowDirectionalLightCount];

    static int directionalShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");

    public void Setup(ScriptableRenderContext context , CullingResults cullingResults , ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        this.context = context;
        this.shadowSettings = shadowSettings;
        shadowedDirectionalLightCount = 0;
    }



    public void ReserveDirectionalShadows(Light light , int visibleLightIndex)
    {
        if (shadowedDirectionalLightCount < maxShadowDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            cullingResults.GetShadowCasterBounds(visibleLightIndex,out Bounds bounds))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount++] =
                new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
        }
    }

    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            cmd.GetTemporaryRT(directionalShadowAtlasID, 1, 1,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)shadowSettings.directional.atlasSize;
        cmd.GetTemporaryRT(directionalShadowAtlasID, atlasSize, atlasSize,
            32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        cmd.SetRenderTarget(directionalShadowAtlasID,
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        cmd.ClearRenderTarget(true, false, Color.clear);

        cmd.BeginSample(bufferName);
        ExcuteBuffer();

        int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tilingSize = atlasSize / split;

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i , split , tilingSize);
        }

        cmd.EndSample(bufferName);
        ExcuteBuffer();
    }

    void RenderDirectionalShadows(int index , int split , int tilingSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings
            (cullingResults, light.visibleLightIndex );//, BatchCullingProjectionType.Orthographic

        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex , 0 , 1 , Vector3.zero , tilingSize , 0f ,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix , out ShadowSplitData splitData
            );

        shadowSettings.splitData = splitData;

        SetTilingViewport(index, split, tilingSize);
        cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        ExcuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }

    void SetTilingViewport(int index , int split , float tilingSize)
    {
        Vector2 offset = new Vector2(index % split, index % split);
        cmd.SetViewport(new Rect(offset.x * tilingSize, offset.y * tilingSize, tilingSize, tilingSize));
    }

    void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        cmd.ReleaseTemporaryRT(directionalShadowAtlasID);
        ExcuteBuffer();
    }
}