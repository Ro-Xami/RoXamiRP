using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiPost
{
    public bool IsActive => renderer != null;
    
    const string bufferName = "RoXamiPost";
    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };
    ScriptableRenderContext context;
    Camera camera;
    RoXamiRenderer renderer;
    
    static readonly int postSource0Id = Shader.PropertyToID("_PostSource0");
    static readonly int postSource1Id = Shader.PropertyToID("_PostSource1");
    static readonly int bloomFilterID = Shader.PropertyToID("_BloomFilter");

    enum Pass
    {
        copy,
        filter,
        blurH,
        blurV,
        upSample,
        combine
    };

    public void Setup(
        ScriptableRenderContext context , Camera camera  , RoXamiRenderer renderer)
    {
        this.context = context;
        this.camera = camera;
        this.renderer = camera.cameraType <= CameraType.SceneView ? renderer : null;
    }

    public void Render(int sourceID)
    {
        //Draw(sourceID, BuiltinRenderTextureType.CameraTarget, Pass.copy);
        
        cmd.BeginSample("RoXami Bloom");
        SetupBloom(sourceID);
        cmd.EndSample("RoXami Bloom");
        
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    void SetupBloom(int sourceID)
    {
        RoXamiRenderer.BloomSettings bloom = renderer.bloomSettings;
        int sampleCount = bloom.maxSampleCount;

        if (sampleCount <= 0)
        {
            return;
        }

        int width = CameraRender.renderingData.width;
        int height = CameraRender.renderingData.height;
        RenderTextureFormat format = RenderTextureFormat.Default;
        FilterMode filter = FilterMode.Bilinear;
        
        //Filter
        cmd.GetTemporaryRT(bloomFilterID , width, height,0,filter , format);
        Draw(sourceID , bloomFilterID, Pass.filter);
        
        //DownSample
        int[] bloomDownSampleIDs = new int[sampleCount];
        int[] bloomUpSampleIDs = new int[sampleCount];
        int fromID = bloomFilterID;
        for (int i = 0; i < sampleCount; i++)
        {
            if (width <= 0 || height <= 0)
            {
                sampleCount = i;
                break;
            }
            bloomDownSampleIDs[i] = Shader.PropertyToID("_BloomDownSample" + i);
            bloomUpSampleIDs[i] = Shader.PropertyToID("_BloomUpSample" + i);
            
            cmd.GetTemporaryRT(bloomDownSampleIDs[i] , width , height, 0, filter , format);
            cmd.GetTemporaryRT(bloomUpSampleIDs[i] , width , height, 0, filter , format);
            
            Draw(fromID , bloomDownSampleIDs[i] , Pass.blurH);
            Draw(bloomDownSampleIDs[i] , bloomUpSampleIDs[i] , Pass.blurV);

            fromID = bloomUpSampleIDs[i];
            width /= 2;
            height /= 2;
        }
        cmd.ReleaseTemporaryRT(bloomFilterID);
        
        //UpSample
        for (int i = sampleCount - 1; i > 0; i--)
        {
            cmd.SetGlobalTexture(postSource1Id , bloomUpSampleIDs[i - 1]);
            Draw(bloomUpSampleIDs[i] , bloomUpSampleIDs[i - 1] , Pass.upSample);
        }
        cmd.SetGlobalTexture(postSource1Id , bloomUpSampleIDs[0]);
        Draw(sourceID , BuiltinRenderTextureType.CameraTarget , Pass.combine);
        
        for (int i = 0; i < sampleCount; i++)
        {
            cmd.ReleaseTemporaryRT(bloomDownSampleIDs[i]);
            cmd.ReleaseTemporaryRT(bloomUpSampleIDs[i]);
        }
    }
    
    void Draw (
        RenderTargetIdentifier from, RenderTargetIdentifier to, Pass pass
    ) {
        cmd.SetGlobalTexture(postSource0Id, from);
        cmd.SetRenderTarget(
            to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
        );
        cmd.DrawProcedural(
            Matrix4x4.identity, renderer.Material, (int)pass,
            MeshTopology.Triangles, 3
        );
    }
}