using System.IO;
using UnityEditor;
using UnityEngine;

namespace RoXamiRenderPipeline
{
    public class LutMaker : EditorWindow
    {
        private Gradient lut;
        private Texture2D tex;

        private const int width = 512;
        private const int height = 2;

        [MenuItem("RoXamiRP Tools/LutMaker")]
        public static void ShowWindow()
        {
            GetWindow<LutMaker>().titleContent = new GUIContent("Lut Maker");
        }

        private void OnEnable()
        {
            if (lut == null)
            {
                lut = new Gradient
                {
                    colorKeys = new[]
                    {
                        new GradientColorKey(Color.black, 0f),
                        new GradientColorKey(Color.white, 1f)
                    },
                    alphaKeys = new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 1f)
                    }
                };
            }

            if (tex == null)
            {
                tex = GenerateLutTexture();
            }
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            tex = EditorGUILayout.ObjectField("Texture", tex, typeof(Texture2D), false) as Texture2D;
            lut = EditorGUILayout.GradientField("Lut", lut);

            if (EditorGUI.EndChangeCheck())
            {
                if (tex != null)
                {
                    ApplyLutToTextureAsset(tex);
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Create Lut"))
            {
                var newTex = GenerateLutTexture();
                SaveTextureAsPng(newTex);
            }

            GUILayout.Space(10);
            GUILayout.Label("Preview", EditorStyles.boldLabel);

            var previewTex = GenerateLutTexture();
            Rect previewRect = GUILayoutUtility.GetRect(width, 32, GUILayout.ExpandWidth(true));
            EditorGUI.DrawPreviewTexture(previewRect, previewTex, null, ScaleMode.StretchToFill);
        }

        private Texture2D GenerateLutTexture()
        {
            Texture2D lutTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            Color[] colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float t = (float)x / (width - 1);
                    colors[x + y * width] = lut.Evaluate(t);
                }
            }
            lutTex.SetPixels(colors);
            lutTex.Apply();
            return lutTex;
        }

        private void ApplyLutToTextureAsset(Texture2D targetTex)
        {
            string assetPath = AssetDatabase.GetAssetPath(targetTex);
            if (!string.IsNullOrEmpty(assetPath))
            {
                File.WriteAllBytes(assetPath, GenerateLutTexture().EncodeToPNG());
                AssetDatabase.Refresh();
            }
        }

        private void SaveTextureAsPng(Texture2D newTexture)
        {
            string savePath = EditorUtility.SaveFilePanel("Save LUT Texture", "Assets", "Lut", "png");
            if (!string.IsNullOrEmpty(savePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                File.WriteAllBytes(savePath, newTexture.EncodeToPNG());
                AssetDatabase.Refresh();

                string assetPath = "Assets" + savePath.Replace(Application.dataPath, "").Replace("\\", "/");
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }
        }
    }
}
