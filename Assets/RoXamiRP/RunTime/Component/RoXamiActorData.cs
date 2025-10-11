using System;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    [ExecuteInEditMode]
    public class RoXamiActorData : MonoBehaviour
    {
        private static readonly int faceFrontDir = Shader.PropertyToID("_faceFrontDir");
        
        [SerializeField] Transform faceTransform;
        [SerializeField] private Material faceMaterial;

        private void OnEnable()
        {
            GetData();
        }

        private void OnValidate()
        {
            GetData();
        }

        private void Update()
        {
            if (faceTransform && faceMaterial)
            {
                faceMaterial.SetVector(faceFrontDir, faceTransform.forward);
            }
        }

        void GetData()
        {
            
        }
    }
}