using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class GBuffer
{
    const string bufferName = "RoXamiGBuffer";

    private CommandBuffer cmd = new CommandBuffer()
    {
        name = bufferName
    };
    
    static int[] GbufferNameIds = new int[]
    {
        Shader.PropertyToID("Gbuffer0"),
        Shader.PropertyToID("Gbuffer1"),
        Shader.PropertyToID("Gbuffer2"),
        Shader.PropertyToID("Gbuffer3"),
    };
    
    public static RenderTargetIdentifier[] GbufferIds = new RenderTargetIdentifier[]
    {
        new RenderTargetIdentifier(GbufferNameIds[0]),
        new RenderTargetIdentifier(GbufferNameIds[1]),
        new RenderTargetIdentifier(GbufferNameIds[2]),
        new RenderTargetIdentifier(GbufferNameIds[3])
    };

    //private static readonly int cameraColorRTID = Shader.PropertyToID("CameraColorAttachment");
    private static readonly int camerDepthRTID = Shader.PropertyToID("CameraDepthAttachment");
    //private readonly RenderTargetIdentifier camerColorRT = new RenderTargetIdentifier(cameraColorRTID);
    private readonly RenderTargetIdentifier cameDepthRT = new RenderTargetIdentifier(camerDepthRTID);
    
     public void SetUp(ScriptableRenderContext context)
    {
    cmd.GetTemporaryRT(camerDepthRTID, CameraRender.renderingData.width, CameraRender.renderingData.height, 24,
        FilterMode.Point,
        RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
    
    // cmd.GetTemporaryRT(cameraColorRTID, CameraRender.renderingData.width, CameraRender.renderingData.height, 24,
    //     FilterMode.Point, GraphicsFormat.B10G11R11_UFloatPack32);
    
    // cmd.SetRenderTarget(camerColorRT, RenderBufferLoadAction.DontCare,
    //     RenderBufferStoreAction.Store,
    //     camerDepthRTID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
    //cmd.ClearRenderTarget(true , true , Color.clear);
        
        
        RenderTextureDescriptor gbufferdesc =
            new RenderTextureDescriptor(CameraRender.renderingData.width, CameraRender.renderingData.height, RenderTextureFormat.ARGB32);
        gbufferdesc.depthBufferBits = 0; //确保没有深度buffer
        gbufferdesc.stencilFormat = GraphicsFormat.None; //模板缓冲区不指定格式
        gbufferdesc.graphicsFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
            ? GraphicsFormat.R8G8B8A8_SRGB
            : GraphicsFormat.R8G8B8A8_UNorm; //根据颜色空间来决定diffusebuffer的RT格式
        cmd.GetTemporaryRT(GbufferNameIds[0], gbufferdesc); //Albedo
        gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        cmd.GetTemporaryRT(GbufferNameIds[1], gbufferdesc); //normal
        gbufferdesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
        cmd.GetTemporaryRT(GbufferNameIds[2], gbufferdesc); //Metallic/AO/DetailMap/Smothness
        gbufferdesc.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
        cmd.GetTemporaryRT(GbufferNameIds[3], gbufferdesc); //HDRGI
        //Gbuffer布局以这里为准
        cmd.SetRenderTarget(GbufferIds, cameDepthRT);
        context.ExecuteCommandBuffer(cmd);
        //cmd.ClearRenderTarget(true , true , Color.clear);//提交完记得清空
    }

    public void Render(ScriptableRenderContext context, RoXamiRenderer renderer)
    {
        cmd.BeginSample(bufferName);
        cmd.DrawProcedural(
            Matrix4x4.identity, renderer.DeferredMaterial, 0,
            MeshTopology.Triangles, 3
        );
        cmd.SetRenderTarget(
            BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        cmd.EndSample(bufferName);
    }

    public void CleanUp()
    {
        for (int i = 0; i < GbufferNameIds.Length; i++)
        {
            cmd.ReleaseTemporaryRT(GbufferNameIds[i]);
        }
        cmd.ReleaseTemporaryRT(camerDepthRTID);
        //cmd.ReleaseTemporaryRT(cameraColorRTID);
    }
}