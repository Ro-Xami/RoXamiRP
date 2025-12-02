using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MaterialBatchConverter : EditorWindow
{
    [MenuItem("RoXamiTools/Material Batch Converter")]
    public static void ShowWindow()
    {
        var window = GetWindow<MaterialBatchConverter>("Material Batch Converter");
        window.minSize = new Vector2(800, 600);
    }

    // 数据类
    [Serializable]
    public class ShaderMappingData
    {
        public Shader sourceShader;
        public Shader targetShader;
        public bool enabled = true;
        public List<MaterialParameterMapping> parameterMappings = new List<MaterialParameterMapping>();
        public List<Material> materials = new List<Material>();
    }

    [Serializable]
    public class MaterialParameterMapping
    {
        public string sourceParameterName;
        public string targetParameterName;
        public bool enabled = true;
        public ParameterType parameterType;
    }

    public enum ParameterType
    {
        Float,
        Vector,
        Texture,
        Color
    }

    // 序列化数据
    [SerializeField] private string searchDirectory = "Assets";
    [SerializeField] private List<ShaderMappingData> shaderMappings = new List<ShaderMappingData>();
    [SerializeField] private Shader targetShader;

    // UI状态
    private Vector2 scrollPosition;
    private Dictionary<Shader, bool> showMaterialList = new Dictionary<Shader, bool>();

    private void OnGUI()
    {
        DrawHeader();
        DrawSearchSection();
        DrawShaderMappingSection();
        DrawActionButtons();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Material Batch Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();
    }

    private void DrawSearchSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Material Search", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        searchDirectory = EditorGUILayout.TextField("Search Directory", searchDirectory);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string newPath = EditorUtility.OpenFolderPanel("Select Directory", searchDirectory, "");
            if (!string.IsNullOrEmpty(newPath))
            {
                searchDirectory = "Assets" + newPath.Replace(Application.dataPath, "");
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Scan Materials"))
        {
            ScanMaterials();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawShaderMappingSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Shader Mapping", EditorStyles.boldLabel);

        // 目标Shader配置
        EditorGUILayout.BeginHorizontal();
        targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", targetShader, typeof(Shader), false);
        if (GUILayout.Button("Apply to All", GUILayout.Width(80)) && targetShader != null)
        {
            foreach (var mapping in shaderMappings)
            {
                mapping.targetShader = targetShader;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Shader分类列表
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < shaderMappings.Count; i++)
        {
            DrawShaderMapping(shaderMappings[i], i);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawShaderMapping(ShaderMappingData mapping, int index)
    {
        EditorGUILayout.BeginVertical("box");

        // Shader信息行
        EditorGUILayout.BeginHorizontal();
        mapping.enabled = EditorGUILayout.Toggle(mapping.enabled, GUILayout.Width(20));
        EditorGUILayout.LabelField($"{mapping.sourceShader?.name ?? "Unknown"} ({mapping.materials.Count} materials)", EditorStyles.boldLabel);

        // 初始化显示状态
        if (!showMaterialList.ContainsKey(mapping.sourceShader))
        {
            showMaterialList[mapping.sourceShader] = false;
        }

        if (GUILayout.Button(showMaterialList[mapping.sourceShader] ? "Hide Materials" : "Show Materials", GUILayout.Width(100)))
        {
            showMaterialList[mapping.sourceShader] = !showMaterialList[mapping.sourceShader];
        }

        if (GUILayout.Button("Convert This", GUILayout.Width(80)))
        {
            ConvertSingleShaderMapping(mapping);
        }

        if (GUILayout.Button("Remove", GUILayout.Width(60)))
        {
            shaderMappings.RemoveAt(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        // 目标Shader配置
        mapping.targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", mapping.targetShader, typeof(Shader), false);

        // 材质列表（可折叠）
        if (showMaterialList[mapping.sourceShader])
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Materials:", EditorStyles.miniBoldLabel);
            foreach (var material in mapping.materials)
            {
                EditorGUILayout.ObjectField(material, typeof(Material), false);
            }
            EditorGUI.indentLevel--;
        }

        // 参数映射
        DrawParameterMappings(mapping);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawParameterMappings(ShaderMappingData mapping)
    {
        if (mapping.materials.Count == 0 || mapping.targetShader == null) return;

        // 获取目标Shader的所有参数
        var targetPropertyNames = GetShaderPropertyNames(mapping.targetShader);

        // 获取第一个材质的参数作为参考
        var referenceMaterial = mapping.materials[0];
        var propertyCount = ShaderUtil.GetPropertyCount(referenceMaterial.shader);

        EditorGUILayout.LabelField("Parameter Mappings:", EditorStyles.miniBoldLabel);

        for (int i = 0; i < propertyCount; i++)
        {
            var propertyName = ShaderUtil.GetPropertyName(referenceMaterial.shader, i);
            var propertyType = ShaderUtil.GetPropertyType(referenceMaterial.shader, i);

            // 转换为我们的参数类型
            ParameterType paramType = ParameterType.Float;
            switch (propertyType)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    paramType = ParameterType.Color;
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    paramType = ParameterType.Vector;
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    paramType = ParameterType.Texture;
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                case ShaderUtil.ShaderPropertyType.Range:
                    paramType = ParameterType.Float;
                    break;
            }

            // 查找或创建映射
            var mappingEntry = mapping.parameterMappings.FirstOrDefault(m => m.sourceParameterName == propertyName);
            if (mappingEntry == null)
            {
                mappingEntry = new MaterialParameterMapping
                {
                    sourceParameterName = propertyName,
                    parameterType = paramType,
                    targetParameterName = propertyName // 默认使用相同名称
                };
                mapping.parameterMappings.Add(mappingEntry);
            }

            EditorGUILayout.BeginHorizontal();
            mappingEntry.enabled = EditorGUILayout.Toggle(mappingEntry.enabled, GUILayout.Width(20));
            EditorGUILayout.LabelField(propertyName, GUILayout.Width(150));
            EditorGUILayout.LabelField(paramType.ToString(), GUILayout.Width(80));

            // 使用下拉菜单选择目标参数
            int selectedIndex = Array.IndexOf(targetPropertyNames, mappingEntry.targetParameterName);
            if (selectedIndex < 0) selectedIndex = 0;

            selectedIndex = EditorGUILayout.Popup(selectedIndex, targetPropertyNames);
            mappingEntry.targetParameterName = targetPropertyNames[selectedIndex];

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Convert Materials"))
        {
            ConvertMaterials();
        }
        if (GUILayout.Button("Clear All"))
        {
            shaderMappings.Clear();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ScanMaterials()
    {
        shaderMappings.Clear();

        // 查找所有材质
        var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { searchDirectory });
        var materialsByShader = new Dictionary<Shader, List<Material>>();

        foreach (var guid in materialGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null && material.shader != null)
            {
                if (!materialsByShader.ContainsKey(material.shader))
                {
                    materialsByShader[material.shader] = new List<Material>();
                }
                materialsByShader[material.shader].Add(material);
            }
        }

        // 创建Shader映射数据
        foreach (var kvp in materialsByShader)
        {
            shaderMappings.Add(new ShaderMappingData
            {
                sourceShader = kvp.Key,
                materials = kvp.Value,
                targetShader = targetShader // 使用全局目标Shader
            });
        }

        // 按材质数量排序
        shaderMappings = shaderMappings.OrderByDescending(m => m.materials.Count).ToList();

        Debug.Log($"Found {materialGuids.Length} materials, grouped into {shaderMappings.Count} shader types.");
    }

    private string[] GetShaderPropertyNames(Shader shader)
    {
        if (shader == null) return new string[0];

        var propertyCount = ShaderUtil.GetPropertyCount(shader);
        var propertyNames = new string[propertyCount];

        for (int i = 0; i < propertyCount; i++)
        {
            propertyNames[i] = ShaderUtil.GetPropertyName(shader, i);
        }

        return propertyNames;
    }

    private void ConvertSingleShaderMapping(ShaderMappingData mapping)
    {
        if (mapping.targetShader == null)
        {
            EditorUtility.DisplayDialog("Error", "Please set target shader first.", "OK");
            return;
        }

        if (mapping.materials.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No materials found for this shader.", "OK");
            return;
        }

        int totalConverted = 0;
        int totalSkipped = 0;
        var failedMaterials = new List<string>();

        try
        {
            for (int i = 0; i < mapping.materials.Count; i++)
            {
                var material = mapping.materials[i];

                // 显示进度
                if (EditorUtility.DisplayCancelableProgressBar(
                    $"Converting {mapping.sourceShader?.name ?? "Unknown"}",
                    $"Converting {material.name} ({i + 1}/{mapping.materials.Count})",
                    (float)(i + 1) / mapping.materials.Count))
                {
                    // 用户取消了操作
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Conversion Cancelled",
                        $"Conversion was cancelled. Converted {totalConverted} materials.", "OK");
                    return;
                }

                try
                {
                    CopyMaterialParameters(material, mapping);
                    totalConverted++;
                    EditorUtility.SetDirty(material);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to convert material {material.name}: {e.Message}");
                    failedMaterials.Add($"{material.name}: {e.Message}");
                    totalSkipped++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        // 显示结果
        string resultMessage = $"Successfully converted {totalConverted} materials.";
        if (totalSkipped > 0)
        {
            resultMessage += $"\nSkipped {totalSkipped} materials due to errors.";
            if (failedMaterials.Count > 0)
            {
                resultMessage += "\n\nFailed materials:\n" + string.Join("\n", failedMaterials.Take(5));
                if (failedMaterials.Count > 5)
                {
                    resultMessage += $"\n... and {failedMaterials.Count - 5} more. Check console for details.";
                }
            }
        }

        EditorUtility.DisplayDialog("Conversion Complete", resultMessage, "OK");
    }

    private void ConvertMaterials()
    {
        int totalMaterials = shaderMappings.Where(m => m.enabled && m.targetShader != null).Sum(m => m.materials.Count);
        if (totalMaterials == 0)
        {
            EditorUtility.DisplayDialog("No Materials", "No materials selected for conversion. Please enable at least one shader mapping and set target shader.", "OK");
            return;
        }

        int currentMaterial = 0;
        int totalConverted = 0;
        int totalSkipped = 0;
        var failedMaterials = new List<string>();

        try
        {
            foreach (var mapping in shaderMappings)
            {
                if (!mapping.enabled || mapping.targetShader == null) continue;

                foreach (var material in mapping.materials)
                {
                    currentMaterial++;

                    // 显示进度
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Converting Materials",
                        $"Converting {material.name} ({currentMaterial}/{totalMaterials})",
                        (float)currentMaterial / totalMaterials))
                    {
                        // 用户取消了操作
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("Conversion Cancelled",
                            $"Conversion was cancelled. Converted {totalConverted} materials.", "OK");
                        return;
                    }

                    try
                    {
                        CopyMaterialParameters(material, mapping);
                        totalConverted++;
                        EditorUtility.SetDirty(material);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to convert material {material.name}: {e.Message}");
                        failedMaterials.Add($"{material.name}: {e.Message}");
                        totalSkipped++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        // 显示结果
        string resultMessage = $"Successfully converted {totalConverted} materials.";
        if (totalSkipped > 0)
        {
            resultMessage += $"\nSkipped {totalSkipped} materials due to errors.";
            if (failedMaterials.Count > 0)
            {
                resultMessage += "\n\nFailed materials:\n" + string.Join("\n", failedMaterials.Take(10));
                if (failedMaterials.Count > 10)
                {
                    resultMessage += $"\n... and {failedMaterials.Count - 10} more. Check console for details.";
                }
            }
        }

        EditorUtility.DisplayDialog("Conversion Complete", resultMessage, "OK");
    }

    // This method caches all source parameters, switches shader, then applies cached values to target parameter names.
    private void CopyMaterialParameters(Material material, ShaderMappingData mapping)
    {
        if (material == null || mapping == null) return;

        // cache dictionaries
        var floatValues = new Dictionary<string, float>();
        var vectorValues = new Dictionary<string, Vector4>();
        var colorValues = new Dictionary<string, Color>();
        var textureValues = new Dictionary<string, Texture>();
        var textureScale = new Dictionary<string, Vector2>();
        var textureOffset = new Dictionary<string, Vector2>();

        // cache shader keywords and renderQueue
        var originalKeywords = material.shaderKeywords;
        int originalRenderQueue = material.renderQueue;

        // 1) Cache source parameter values (material still has original shader)
        foreach (var param in mapping.parameterMappings)
        {
            if (!param.enabled) continue;

            string src = param.sourceParameterName;

            if (!material.HasProperty(src)) continue;

            try
            {
                switch (param.parameterType)
                {
                    case ParameterType.Float:
                        floatValues[src] = material.GetFloat(src);
                        break;
                    case ParameterType.Vector:
                        vectorValues[src] = material.GetVector(src);
                        break;
                    case ParameterType.Color:
                        colorValues[src] = material.GetColor(src);
                        break;
                    case ParameterType.Texture:
                        textureValues[src] = material.GetTexture(src);
                        textureScale[src] = material.GetTextureScale(src);
                        textureOffset[src] = material.GetTextureOffset(src);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to read property '{src}' from material '{material.name}': {e.Message}");
            }
        }

        // 2) Switch shader
        material.shader = mapping.targetShader;

        // 3) Apply cached values to target parameter names if target has those properties
        foreach (var param in mapping.parameterMappings)
        {
            if (!param.enabled) continue;

            string src = param.sourceParameterName;
            string dst = param.targetParameterName;

            if (!material.HasProperty(dst)) continue;

            try
            {
                switch (param.parameterType)
                {
                    case ParameterType.Float:
                        if (floatValues.TryGetValue(src, out var fVal))
                            material.SetFloat(dst, fVal);
                        break;
                    case ParameterType.Vector:
                        if (vectorValues.TryGetValue(src, out var vVal))
                            material.SetVector(dst, vVal);
                        break;
                    case ParameterType.Color:
                        if (colorValues.TryGetValue(src, out var cVal))
                            material.SetColor(dst, cVal);
                        break;
                    case ParameterType.Texture:
                        if (textureValues.TryGetValue(src, out var tVal))
                            material.SetTexture(dst, tVal);

                        // copy scale/offset if present
                        if (textureScale.TryGetValue(src, out var ts))
                            material.SetTextureScale(dst, ts);
                        if (textureOffset.TryGetValue(src, out var to)
                            )
                            material.SetTextureOffset(dst, to);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to set property '{dst}' on material '{material.name}': {e.Message}");
            }
        }

        // 4) restore keywords and renderQueue (optional - keep original keywords)
        try
        {
            material.shaderKeywords = originalKeywords;
        }
        catch { /* ignore */ }

        material.renderQueue = originalRenderQueue;
    }
}
