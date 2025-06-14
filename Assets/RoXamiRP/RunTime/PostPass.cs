using UnityEngine;
using UnityEngine.Rendering;

public class PostPass
{
    public bool IsActive => renderer != null;
    
    const string bufferName = "RoXami Post";
    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName,
    };
    ScriptableRenderContext context;
    Camera camera;
    bool isHDR;
    RoXamiRenderer renderer;
    
    static readonly int postSource0Id = Shader.PropertyToID("_PostSource0");
    static readonly int postSource1Id = Shader.PropertyToID("_PostSource1");
    static readonly int bloomFilterID = Shader.PropertyToID("_BloomFilter");
    static readonly int bloomParam = Shader.PropertyToID("_bloomParam");
    static readonly int bloomIntensity = Shader.PropertyToID("_bloomIntensity");

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
        ScriptableRenderContext context , Camera camera  , RoXamiRenderer renderer, bool isHDR)
    {
        this.context = context;
        this.camera = camera;
        this.renderer = camera.cameraType <= CameraType.SceneView ? renderer : null;
        this.isHDR = isHDR;
    }

    public void Render()
    {
        cmd.BeginSample("RoXami Bloom");
        SetupBloom(CameraRender.renderingData.cameraColorBufferId);
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

        //get the rt size and format
        int width = CameraRender.renderingData.width;
        int height = CameraRender.renderingData.height;
        RenderTextureFormat format = isHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        FilterMode filter = FilterMode.Bilinear;
        
        //Set bloom Shader datas
        float threshold = Mathf.GammaToLinearSpace(bloom.threshold);
        float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee
        cmd.SetGlobalVector(bloomParam , new Vector4(threshold , thresholdKnee, bloom.clampMax , bloom.scatter));
        cmd.SetGlobalFloat(bloomIntensity , bloom.intensity);
        
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
            //lower the RT's size to the half size, and set Bilinear filterMode, to ues the unity's mipmap
            //a single mipmap0 1x1 pixel is the mipmap1 2x2 pixel's average color
            //example ,when size/2 and blur size is 3x3, same as the blur size is 6x6
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
            Matrix4x4.identity, renderer.PostMaterial, (int)pass,
            MeshTopology.Triangles, 3
        );
    }
}