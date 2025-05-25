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

    //这样其实是不安全的，如果GbufferIds在GbufferNameIds之前初始化，那么就会有错误了。但是据说C#中是按顺序初始化的
    public static RenderTargetIdentifier[] GbufferIds = new RenderTargetIdentifier[]
    {
        new RenderTargetIdentifier(GbufferNameIds[0]),
        new RenderTargetIdentifier(GbufferNameIds[1]),
        new RenderTargetIdentifier(GbufferNameIds[2]),
        new RenderTargetIdentifier(GbufferNameIds[3])
    };

    private static readonly int cameraColorRTID = Shader.PropertyToID("CameraColorAttachment");
    private static readonly int camerDepthRTID = Shader.PropertyToID("CameraDepthAttachment");
    private readonly RenderTargetIdentifier camerColorRT = new RenderTargetIdentifier(cameraColorRTID);
    private readonly RenderTargetIdentifier cameDepthRT = new RenderTargetIdentifier(camerDepthRTID);
    
     public void GBufferPass(ScriptableRenderContext context)
    {
    cmd.GetTemporaryRT(camerDepthRTID, Screen.width, Screen.height, 24,
        FilterMode.Point,
        RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
    
    cmd.GetTemporaryRT(cameraColorRTID, Screen.width, Screen.height, 24,
        FilterMode.Point, GraphicsFormat.B10G11R11_UFloatPack32);
    
    cmd.SetRenderTarget(camerColorRT, RenderBufferLoadAction.DontCare,
        RenderBufferStoreAction.Store,
        camerDepthRTID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
    cmd.ClearRenderTarget(true , true , Color.clear);
        
        
        RenderTextureDescriptor gbufferdesc =
            new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32);
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
        cmd.ClearRenderTarget(true , true , Color.clear);//提交完记得清空
    }

    public void CleanUp()
    {
        for (int i = 0; i < GbufferNameIds.Length; i++)
        {
            cmd.ReleaseTemporaryRT(GbufferNameIds[i]);
        }
        cmd.ReleaseTemporaryRT(camerDepthRTID);
        cmd.ReleaseTemporaryRT(cameraColorRTID);
    }
}