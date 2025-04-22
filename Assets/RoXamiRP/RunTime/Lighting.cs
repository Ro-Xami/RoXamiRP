using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const int lightIndex = 0;
    const string bufferName = "ToonLighting";
    CullingResults cullingResults;

    Shadows shadows = new Shadows();

    static int
        dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

    static Vector4
        dirLightColor, dirLightDirection;

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

        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupDirectionalLight()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

        //≥ı ºªØ
        Light light = null;
        dirLightColor = Vector4.zero;
        dirLightDirection = Vector4.zero;

        if (visibleLights != null && visibleLights.Length != 0 && visibleLights[lightIndex].lightType == LightType.Directional)
        {
            VisibleLight visibleLight = visibleLights[lightIndex];
            light = visibleLight.light;

            dirLightColor = visibleLight.finalColor;
            dirLightDirection = -visibleLight.localToWorldMatrix.GetColumn(2);
        }
        buffer.SetGlobalVector(dirLightColorId, dirLightColor);
        buffer.SetGlobalVector(dirLightDirectionId, dirLightDirection);

//#if UNITY_EDITOR
//        if (camera.cameraType == CameraType.Game) { shadows.Render(light); }
//#else
        shadows.Render(light);
//#endif

    }

    public void CleanUp()
    {
        shadows.CleanUp();
    }
}