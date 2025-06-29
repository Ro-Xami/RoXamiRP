using UnityEngine;
using UnityEngine.Rendering;

public class ScreenSpaceShadowsPass
{
    const string bufferName = "RoXami ScreenSpaceShadows";
    private static readonly CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName,
    };
    
    private static readonly string[] directionalFilterKeywords =
    {
        "_DIRECTIONAL_PCF3",
        "_DIRECTIONAL_PCF5",
        "_DIRECTIONAL_PCF7",
    };
    
    private static readonly int
        directionalLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"),
        directionalShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices"),
        cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
        shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
    
    static readonly int screenSpaceShadowsTextureID = Shader.PropertyToID("_ScreenSpaceShadowsTexture");
    static readonly int textureSizeID = Shader.PropertyToID("_TextureSize");
    private const string kernelName = "ScreenSpaceShadows";
    RenderingData renderingData;

    public void SetUp(RenderingData renderData)
    {
        renderingData = renderData;
        int width = renderingData.width;
        int height = renderingData.height;

        cmd.GetTemporaryRT(screenSpaceShadowsTextureID,
            width, height, 0, FilterMode.Point, RenderTextureFormat.R16,
            RenderTextureReadWrite.Linear, 1, true);
        cmd.SetRenderTarget(screenSpaceShadowsTextureID, 
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        cmd.BeginSample(bufferName);
        ExecuteBuffer();
        
        ComputeScreenSpaceShadows(width, height, renderingData.screenSpaceShadowsData);

        cmd.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void ComputeScreenSpaceShadows(int width, int height, ScreenSpaceShadowsData screenSpaceShadowsData)
    {
        ComputeShader cs = renderingData.renderer.screenSpaceShadowsCompute;
        int kernel = cs.FindKernel(kernelName);

        cmd.SetComputeVectorParam(cs,
            textureSizeID, new Vector4(width, height, 1f / width, 1f / height));
        cmd.SetComputeVectorParam(cs,
            directionalLightShadowDataId, screenSpaceShadowsData.directionalLightShadowData);
        cmd.SetComputeVectorArrayParam(cs,
            cascadeCullingSpheresId, screenSpaceShadowsData.cascadeCullingSpheres);
        cmd.SetComputeMatrixArrayParam(cs,
            directionalShadowMatricesID, screenSpaceShadowsData.directionalShadowMatrices);
        cmd.SetComputeVectorParam(cs,
            shadowDistanceFadeId, screenSpaceShadowsData.shadowDistanceFade);
        cmd.SetComputeTextureParam(cs, kernel,
            screenSpaceShadowsData.directionalShadowAtlasID, screenSpaceShadowsData.directionalShadowAtlasID);
        cmd.SetComputeTextureParam(cs, kernel, 
            screenSpaceShadowsTextureID, screenSpaceShadowsTextureID);
        
        int threadGroupX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupY = Mathf.CeilToInt(height / 8.0f);
        cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
    }
    
    // void SetKeywords()
    // {
    //     int enabledIndex = (int)settings.directional.filter - 1;
    //     for (int i = 0; i < directionalFilterKeywords.Length; i++)
    //     {
    //         if (i == enabledIndex)
    //         {
    //             cmd.compute(directionalFilterKeywords[i]);
    //         }
    //         else
    //         {
    //             cmd.DisableShaderKeyword(directionalFilterKeywords[i]);
    //         }
    //     }
    // }

    public void CleanUp()
    {
        cmd.ReleaseTemporaryRT(screenSpaceShadowsTextureID);
    }

    void ExecuteBuffer()
    {
        renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}