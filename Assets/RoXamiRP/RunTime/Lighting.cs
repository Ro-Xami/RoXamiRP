using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const string bufferName = "ToonLighting";
    CullingResults cullingResults;
    Shadows shadows = new Shadows();

    const int maxDirectionalLightCount = 2;

    static int
        dirLightCount = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

    static Vector4[]
        dirLightColors = new Vector4[maxDirectionalLightCount],
        dirLightDirections = new Vector4[maxDirectionalLightCount];

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    

    public void Setup(ScriptableRenderContext context , CullingResults cullingResults , ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        shadows.Setup(context, cullingResults, shadowSettings);
        SetupDirectionalLight();
        shadows.Render();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupDirectionalLight()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

        int currentDirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if(visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(currentDirLightCount++, ref visibleLight);
                if (currentDirLightCount >= maxDirectionalLightCount)
                {
                    break;
                }
            } 
        }

        buffer.SetGlobalInt(dirLightCount , currentDirLightCount);
        buffer.SetGlobalVectorArray(dirLightColorId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);
    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        shadows.ReserveDirectionalShadows(visibleLight.light , index);
    }

    public void CleanUp()
    {
        shadows.CleanUp();
    }
}