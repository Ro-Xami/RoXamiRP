using UnityEngine.Rendering;
using UnityEngine;

public class DepthToPositionWS
{
    static readonly int worldSpacePositionTextureID = Shader.PropertyToID("_WorldSpacePositionTexture");
    static readonly int worldSpacePositionResultTextureID = Shader.PropertyToID("_Result");
    static readonly int depthTextureID = Shader.PropertyToID("_CameraDepthTexture");
    static readonly int textureSizeID = Shader.PropertyToID("_TextureSize");
    const string depthToWorldPositionKernel =  "DepthToPositionWS";
    
    public void CalculatePositionWS(CommandBuffer cmd, RenderingData renderingData)
    {
        cmd.GetTemporaryRT(
            worldSpacePositionTextureID,
            renderingData.width, renderingData.height,
            0, FilterMode.Point, RenderTextureFormat.ARGBFloat,
            RenderTextureReadWrite.Linear, 1,true);
    
        ComputeShader cs = renderingData.renderer.depthToPositionWSCompute;
        int kernel = cs.FindKernel(depthToWorldPositionKernel);
    
        int width = renderingData.width;
        int height = renderingData.height;
        cmd.SetComputeVectorParam(cs, textureSizeID,
            new Vector4(width, height, 1.0f / width, 1.0f / height));
    
    
        cmd.SetComputeTextureParam(cs, kernel,depthTextureID,renderingData.cameraDepthCopyTextureID);
        cmd.SetComputeTextureParam(cs, kernel,worldSpacePositionResultTextureID,worldSpacePositionTextureID);
    
    
        int threadGroupX = Mathf.CeilToInt(renderingData.width / 8.0f);
        int threadGroupY = Mathf.CeilToInt(renderingData.height / 8.0f);
        cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
    }

    public void CleanUp(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(worldSpacePositionTextureID);
    }
}