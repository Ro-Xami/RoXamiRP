using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoXamiRenderer Asset", menuName ="RoXamiRP/RoXamiRenderer Asset")]
public class RoXamiRendererAsset : ScriptableObject
{
    public CommonSettings commonSettings;

    public BloomSettings bloomSettings;
    
    public List<RoXamiRenderFeature> roXamiRenderFeatures = new List<RoXamiRenderFeature>();
    
    public static RoXamiRendererAsset defaultAsset;

    private void OnEnable()
    {
        if (defaultAsset == null)
        {
            defaultAsset = this;
        }
    }
}

[Serializable]
public class CommonSettings
{
    public bool enableHDR = false;
    public bool enableGpuInstancing = false;
    public bool enableDynamicBatching = false;
    public bool enablePostProcessing = false;
    public bool enableScreenSpaceShadows = false;
}

[Serializable]
public class  BloomSettings
{
    [Min(0f)]public float intensity = 1f;
    [Min(0f)]public float clampMax = 5f;
    [Range(0f , 1f)]public float threshold = 0.9f;
    [Range(0f , 1f)]public float scatter = 0.7f;
    [Range(0f , 10f)]public int maxSampleCount = 5;
}
