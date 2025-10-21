using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    
#if UNITY_EDITOR
    
    public static class EditorTools
    {
        private static Texture2D m_BackgroundTexture;
        public static Texture2D backgroundTexture
        {
            get
            {
                if (m_BackgroundTexture == null)
                {
                    m_BackgroundTexture = Get1x1ColorTexture(new Color(0.17f, 0.17f, 0.2f, 1f));
                }
                return m_BackgroundTexture;
            }
        }
        
        public static List<Type> GetTypesInAssets<T>()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)
                .ToList();
        }
        
        public static Texture2D Get1x1ColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    texture.SetPixel(i, j, color);
                }
            }
            texture.Apply();
            return texture;
        }
    }
        
#endif
    
}