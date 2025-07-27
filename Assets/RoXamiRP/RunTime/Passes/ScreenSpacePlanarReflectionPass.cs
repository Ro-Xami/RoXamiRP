using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class ScreenSpacePlanarReflectionPass
{
    const string bufferName = "RoXami SSPR Pass";
    private readonly CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };
    
    RenderingData renderingData;
    
    ComputeShader compute;
    const string ssprKernelName = "SSPRCompute";
    //const string holeKernelName = "SSPRHole";
    static readonly int ssprTextureID = Shader.PropertyToID("_SSPRTexture");
    static readonly int heightBufferID = Shader.PropertyToID("_SSPRHeightBuffer");
    static readonly int texelSizeID = Shader.PropertyToID("_texelSize");
    static readonly int heightID = Shader.PropertyToID("_height");

    public void SetUp(ScriptableRenderContext context, RenderingData renderData)
    {
        renderingData = renderData;

        CleanUp();
        
        var ssprDescriptor = renderData.cameraData.cameraColorDescriptor;
        ssprDescriptor.enableRandomWrite = true;
        cmd.GetTemporaryRT(ssprTextureID, 
            ssprDescriptor, renderingData.cameraData.cameraColorFilterMode);
        cmd.GetTemporaryRT(heightBufferID, 
            ssprDescriptor, renderingData.cameraData.cameraColorFilterMode);

        cmd.SetRenderTarget(ssprTextureID, 
            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
        cmd.BeginSample(bufferName);
        ExecuteBuffer(context);
        SSPRCompute();
        cmd.EndSample(bufferName);
        ExecuteBuffer(context);
    }
    
    void SSPRCompute()
    {
        compute = renderingData.rendererAsset.screenSpacePlanarReflectionCompute;
        int ssprKernel = compute.FindKernel(ssprKernelName);
        //int holeKernel = compute.FindKernel(holeKernelName);
        
        int width = renderingData.cameraData.width;
        int height = renderingData.cameraData.height;
        
        cmd.SetComputeFloatParam(compute, heightID, 0);
        cmd.SetComputeVectorParam(compute, texelSizeID, 
            new Vector4(width, height, 1 / (float)width, 1 / (float)height));
        cmd.SetComputeTextureParam(compute, ssprKernel, 
            ShaderDataID.cameraDepthCopyTextureID, ShaderDataID.cameraDepthCopyTextureID);
        cmd.SetComputeTextureParam(compute, ssprKernel, 
            ShaderDataID.cameraColorCopyTextureID, ShaderDataID.cameraColorCopyTextureID);
        cmd.SetComputeTextureParam(compute, ssprKernel, 
            heightBufferID, heightBufferID);
        cmd.SetComputeTextureParam(compute, ssprKernel, 
            ssprTextureID, ssprTextureID);
        
        int threadGroupX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupY = Mathf.CeilToInt(height / 8.0f);

        cmd.DispatchCompute(compute, ssprKernel, threadGroupX, threadGroupY, 1);
        //cmd.DispatchCompute(compute, holeKernel, threadGroupX, threadGroupY, 1);
    }

    public void CleanUp()
    {
        cmd.ReleaseTemporaryRT(ssprTextureID);
        cmd.ReleaseTemporaryRT(heightBufferID);
    }

    void ExecuteBuffer(ScriptableRenderContext context)
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}