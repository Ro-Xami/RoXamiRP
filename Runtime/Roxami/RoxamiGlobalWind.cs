using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Roxami
{
    [ExecuteAlways]
    public class RoxmiGlobalWind : MonoBehaviour
    {
        private static RoxmiGlobalWind m_Instance;
        public static RoxmiGlobalWind Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindObjectOfType<RoxmiGlobalWind>();
                    
                    if (!m_Instance)
                    {
                        var go = new GameObject("RoxmiGlobalWind")
                        {
                            hideFlags = HideFlags.HideAndDontSave
                        };
                        m_Instance = go.AddComponent<RoxmiGlobalWind>();
                    }
                }
                return m_Instance;
            }
        }
        
        [SerializeField]
        public GlobalWindSettings settings;
        
        [Serializable]
        public struct GlobalWindSettings
        {
            [Min(0f)] public float windSpeed;
            [Min(0f)] public float windStrength;
            [Min(0f)] public float windNoise;
        }
        
        private static readonly int globalWindDirectionID = Shader.PropertyToID("_globalWindDirection");
        private static readonly int globalWindParams = Shader.PropertyToID("_globalWindParams");

        public void UpdateWind()
        {
            // 使用自身前向朝向作为风方向
            Vector3 windDirection = transform.forward.normalized;
            Shader.SetGlobalVector(globalWindDirectionID, windDirection);
            Shader.SetGlobalVector(globalWindParams, new Vector4(settings.windStrength, settings.windSpeed, settings.windNoise));
        }

        private void OnEnable()
        {
            m_Instance = this;
            UpdateWind();
        }

        private void OnValidate()
        {
            m_Instance = this;
            UpdateWind();
        }

    #if UNITY_EDITOR
        private void Update()
        {
            m_Instance = this;
            UpdateWind();
        }
        
        [MenuItem("GameObject/RoxmiRP/GlobalWind")]
        static void Create()
        {
            GameObject go = new GameObject("RoxmiGlobalWind");
            go.AddComponent<RoxmiGlobalWind>();
            Selection.activeGameObject = go;
        }

        private void OnDrawGizmos()
        {
            // 绘制风方向箭头
            Vector3 position = transform.position;
            Vector3 direction = transform.forward;
            
            // 绘制风方向线
            Gizmos.color = Color.green;
            Gizmos.DrawRay(position, direction);
            
            // 绘制箭头头部
            float arrowHeadLength = 0.5f;
            float arrowHeadAngle = 20f;
            
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            
            Gizmos.DrawRay(position + direction, right * arrowHeadLength);
            Gizmos.DrawRay(position + direction, left * arrowHeadLength);
            
            // 绘制风强度指示器
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, settings.windStrength * 0.1f);
        }
#endif
    }
}


