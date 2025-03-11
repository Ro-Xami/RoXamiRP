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

    const int maxShadowDirectionalLightCount = 1;
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
        //ExcuteBuffer();
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

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i , atlasSize);
        }

        cmd.EndSample(bufferName);
        ExcuteBuffer();
    }

    void RenderDirectionalShadows(int index , int tilingSize)
    {

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