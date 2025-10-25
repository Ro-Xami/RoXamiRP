using System;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    [ExecuteInEditMode]
    public class RoXamiActorData : MonoBehaviour
    {
        private static readonly int faceFrontRightDirID = Shader.PropertyToID("_faceFrontRightDir");
        
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
                faceMaterial.SetVector(faceFrontRightDirID, 
                    new Vector4(faceTransform.forward.x, faceTransform.forward.z, 
                    faceTransform.right.x, faceTransform.right.z));
            }
        }

        void GetData()
        {
            
        }
    }
}