using UnityEngine.Rendering;
using UnityEngine;

public class DepthToPositionWSPass
{
    const string bufferName = "RoXami DepthToPositionWS";
    private static CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };
    
    static readonly int worldSpacePositionTextureID = Shader.PropertyToID("_WorldSpacePositionTexture");
    private RenderingData renderingData;

    public void SetUp(RenderingData renderData)
    {
        renderingData = renderData;
        int width = renderingData.cameraData.width;
        int height = renderingData.cameraData.height;
        
        cmd.GetTemporaryRT(worldSpacePositionTextureID, width, height, 
            0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
        cmd.SetRenderTarget(worldSpacePositionTextureID, 
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        cmd.BeginSample(bufferName);
        ExecuteBuffer();

        cmd.DrawProcedural(
            Matrix4x4.identity, renderData.RendererAsset.depthToPositionWSMaterial,
            0, MeshTopology.Triangles, 3);
        
        cmd.EndSample(bufferName);
        ExecuteBuffer();
    }

    void ExecuteBuffer()
    {
        renderingData.context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void CleanUp()
    {
        cmd.ReleaseTemporaryRT(worldSpacePositionTextureID);
    }
    
    #region ComputeShader
    // static readonly int worldSpacePositionResultTextureID = Shader.PropertyToID("_Result");
    // static readonly int depthTextureID = Shader.PropertyToID("_CameraDepthTexture");
    // static readonly int textureSizeID = Shader.PropertyToID("_TextureSize");
    // const string depthToWorldPositionKernel =  "DepthToPositionWS";
    // public void CalculatePositionWS(CommandBuffer cmd, RenderingData renderingData)
    // {
    //     cmd.GetTemporaryRT(
    //         worldSpacePositionTextureID,
    //         renderingData.width, renderingData.height,
    //         0, FilterMode.Point, RenderTextureFormat.ARGBFloat,
    //         RenderTextureReadWrite.Linear, 1,true);
    //
    //     ComputeShader cs = renderingData.renderer.depthToPositionWSCompute;
    //     int kernel = cs.FindKernel(depthToWorldPositionKernel);
    //
    //     int width = renderingData.width;
    //     int height = renderingData.height;
    //     cmd.SetComputeVectorParam(cs, textureSizeID,
    //         new Vector4(width, height, 1.0f / width, 1.0f / height));
    //
    //
    //     cmd.SetComputeTextureParam(cs, kernel,depthTextureID,renderingData.cameraDepthCopyTextureID);
    //     cmd.SetComputeTextureParam(cs, kernel,worldSpacePositionResultTextureID,worldSpacePositionTextureID);
    //
    //
    //     int threadGroupX = Mathf.CeilToInt(renderingData.width / 8.0f);
    //     int threadGroupY = Mathf.CeilToInt(renderingData.height / 8.0f);
    //     cmd.DispatchCompute(cs, kernel, threadGroupX, threadGroupY, 1);
    // }
    #endregion
}