using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RoXamiRPAsset", menuName = "RoXamiRP/RoXamiRP Asset")]
public class RoXamiRPAsset : RenderPipelineAsset , IDisposable
{
    [SerializeField]
    ShadowSettings shadowSettings = default;
    
    [SerializeField]
    private ShaderAsset shaderAsset = new ShaderAsset();

    protected override RenderPipeline CreatePipeline()
    {
        return new RoXamiRP(shadowSettings, shaderAsset);
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        CoreUtils.Destroy(shaderAsset.postMaterial);
        CoreUtils.Destroy(shaderAsset.deferredMaterial);
    }
}

[Serializable]
public class ShaderAsset
{
    Material post;
    public Material postMaterial
    {
        get 
        {
            if (post == null)
            {
                post = CoreUtils.CreateEngineMaterial("RoXami RP/Hide/Post");
            }
            return post;
        }
    }
    Material deferred;

    public Material deferredMaterial
    {
        get
        {
            if (deferred == null)
            {
                deferred = CoreUtils.CreateEngineMaterial("RoXami RP/Hide/DeferredToonLit");
            }
            return deferred;
        }
    }

    public ComputeShader screenSpaceShadowComputeShader;
}

[System.Serializable]
public class ShadowSettings
{
    [Min(1f)]
    public float maxDistance = 500f;

    [Range(0.001f, 1f)]
    public float distanceFade = 0.1f;

    public Directional directional = new Directional
    {
        atlasSize = MapSize._2048,
        filter = FilterModeSetting.PCF2x2,
        cascadeRotio1 = 0.3f,
        cascadeRotio2 = 0.6f,
        cascadeRotio3 = 0.8f,
        cascadeFade = 0.1f
    };
}

public enum FilterModeSetting
{
    PCF2x2, PCF3x3, PCF5x5, PCF7x7
}

[System.Serializable]
public struct Directional
{
    public MapSize atlasSize;
    public FilterModeSetting filter;

    [Range(0f, 1f)]
    public float cascadeRotio1, cascadeRotio2, cascadeRotio3;

    public Vector3 CascadeRatios => new Vector3(cascadeRotio1, cascadeRotio2, cascadeRotio3);

    [Range(0.001f, 1f)]
    public float cascadeFade;
}

public enum MapSize
{
    _256 = 256,
    _512 = 512,
    _1024 = 1024,
    _2048 = 2048,
    _4096 = 4096,
    _8192 = 8192,
}
