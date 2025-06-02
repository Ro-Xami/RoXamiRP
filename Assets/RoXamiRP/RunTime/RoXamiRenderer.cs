using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoXamiRenderer Asste", menuName ="RoXamiRP/RoXamiRenderer Asste")]
public class RoXamiRenderer : ScriptableObject
{
    [SerializeField]
    Shader shader = default;
    
    [System.NonSerialized]
    Material material;
    public Material Material {
        get {
            if (material == null && shader != null) {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
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

    public BloomSettings bloomSettings;
}
