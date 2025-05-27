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
        public float intensity = 0.1f;
        public float threshold = 0.95f;
        public int maxSampleCount = 5;
    }

    public BloomSettings bloomSettings;
}
