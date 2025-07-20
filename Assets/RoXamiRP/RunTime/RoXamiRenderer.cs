using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoXamiRenderer Asset", menuName ="RoXamiRP/RoXamiRenderer Asset")]
public class RoXamiRenderer : ScriptableObject
{
    public Material depthToPositionWSMaterial;
    public Material postMaterial;
    public Material deferredMaterial;
    public ComputeShader depthToPositionWSCompute;
    public ComputeShader screenSpaceShadowsCompute;
    public ComputeShader screenSpacePlanarReflectionCompute;
    
    [Serializable]
    public class  BloomSettings
    {
        [Min(0f)]public float intensity = 1f;
        [Min(0f)]public float clampMax = 5f;
        [Range(0f , 1f)]public float threshold = 0.9f;
        [Range(0f , 1f)]public float scatter = 0.7f;
        [Range(0f , 10f)]public int maxSampleCount = 5;
    }

    public BloomSettings bloomSettings;
}
