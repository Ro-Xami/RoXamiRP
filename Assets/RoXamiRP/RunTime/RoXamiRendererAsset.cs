using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RoXamiRenderer Asset", menuName ="RoXamiRP/RoXamiRenderer Asset")]
public class RoXamiRendererAsset : ScriptableObject
{
    public CommonSettings commonSettings;

    public List<RoXamiRenderFeature> roXamiRenderFeatures = new List<RoXamiRenderFeature>(10);
    
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
    public bool enableDeferredRendering = true;
    public bool enableScreenSpaceShadows = false;
    public bool enablePostProcessing = false;
}
