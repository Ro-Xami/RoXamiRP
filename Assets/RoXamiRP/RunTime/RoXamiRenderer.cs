using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoXamiRenderer Asste", menuName ="RoXamiRP/RoXamiRenderer Asste")]
public class RoXamiRenderer : ScriptableObject
{
    [SerializeField]
    Shader postShader = default;
    
    [System.NonSerialized]
    Material postMaterial;
    public Material PostMaterial {
        get {
            if (postMaterial == null && postShader != null) {
                postMaterial = new Material(postShader);
                postMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return postMaterial;
        }
    }
    
    [SerializeField]
    Shader deferredShader = default;
    
    [System.NonSerialized]
    Material deferredMaterial;
    public Material DeferredMaterial {
        get {
            if (deferredMaterial == null && deferredShader != null) {
                deferredMaterial = new Material(deferredShader);
                deferredMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return deferredMaterial;
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
