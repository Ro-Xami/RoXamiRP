using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRP
{
    public static class RoXamiRTHandlePool
    {
        private static readonly Dictionary<int, RTHandle> m_RTHandles = new Dictionary<int, RTHandle>(32);
        
        /// <summary>
        /// only get rt when RTHandle input has changed with null or size, and set into the RoXamiRTHandlePool to managed global
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="descriptor"></param>
        /// <param name="name"></param>
        /// <param name="filterMode"></param>
        /// <param name="wrapMode"></param>
        /// <param name="isShadowMap"></param>
        /// <param name="anisoLevel"></param>
        /// <param name="mipMapBias"></param>
        /// <returns></returns>
        public static bool GetRTHandleIfNeeded(
            ref RTHandle handle,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            string name = "",
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0)
        {
            if (RTHandleNeedsReAlloc(handle, descriptor))
            {
                int key = GetRTHandleKey(handle, descriptor, name);
                //Debug.Log("GetRTHandleIfNeeded");
                if (handle == null || !handle.rt)
                {
                    handle = RTHandles.Alloc(descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
                }
                
                if (handle.rt.width != descriptor.width || handle.rt.height != descriptor.height)
                {
                    handle.Release();
                    handle = RTHandles.Alloc(descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
                }

                if (m_RTHandles.TryGetValue(key, out var rtHandle))
                {
                    rtHandle?.Release();
                }
                
                m_RTHandles[key] = handle;
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// use urp function, but dont need to check RenderTextureDescriptor
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        private static bool RTHandleNeedsReAlloc(
            RTHandle handle,
            in RenderTextureDescriptor descriptor)
        {
            if (handle == null || handle.rt == null)
                return true;
            
            if (handle.rt.width != descriptor.width || handle.rt.height != descriptor.height)
                return true;
            
            return false;
        }

        private static int GetRTHandleKey(RTHandle handle, RenderTextureDescriptor descriptor, string name)
        {
            return HashCode.Combine(descriptor.width, descriptor.height, descriptor.colorFormat, name.GetHashCode());
        }

        public static void ReleasePool()
        {
            foreach (var handle in m_RTHandles)
            {
                handle.Value?.Release();
            }
            m_RTHandles.Clear();
        }

        public static int GetRTHandlesCount()
        {
            return m_RTHandles.Count;
        }

        public static void DebugRTHandles()
        {
            Debug.Log("RTHandle Count: " + GetRTHandlesCount());
            foreach (var rtHandle in m_RTHandles)
            {
                Debug.Log(rtHandle.Value.name);
            }
        }
    }
}