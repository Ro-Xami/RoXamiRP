using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RoXamiRenderer
{
    List<RoXamiRenderFeature> features = new List<RoXamiRenderFeature>();
    readonly List<RoXamiRenderPass> activePasses = new List<RoXamiRenderPass>(32);

    private readonly LightingPass lightPass = new LightingPass(RenderPassEvent.BeforeRenderingShadows);
    private readonly RenderingPrePasses prePasses = new RenderingPrePasses(RenderPassEvent.BeforeRenderingPrePasses);
    private readonly GBufferPass gBufferPass =  new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer);
    private readonly DeferredPass deferredPass = new DeferredPass(RenderPassEvent.BeforeRenderingDeferredLights);
    private readonly ScreenSpaceShadowsPass ssShadowsPass =  new ScreenSpaceShadowsPass(RenderPassEvent.AfterRenderingDeferredLights);
    private readonly ForwardPass forwardPass = new ForwardPass(RenderPassEvent.AfterRenderingOpaques);
    private readonly PostPass postPass = new PostPass(RenderPassEvent.BeforeRenderingPostProcessing);

    public RoXamiRenderer()
    {
        CreatePasses();

        SortStable(activePasses);
    }

    public void CameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        foreach (var pass in activePasses)
        {
            pass.SetUp(cmd, ref renderingData);
        }
    }

    public void ExecuteRoXamiRenderPass(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        foreach (var pass in activePasses)
        {
            pass.Execute(context, ref renderingData);
        }
    }

    public void CameraCleanUp()
    {
        foreach (var pass in activePasses)
        {
            pass.CleanUp();
        }
    }

    void CreatePasses()
    {
        activePasses.Clear();
        activePasses.Add(lightPass);
        activePasses.Add(prePasses);
        activePasses.Add(gBufferPass);
        activePasses.Add(deferredPass);
        //activePasses.Add(ssShadowsPass);
        activePasses.Add(forwardPass);
        //activePasses.Add(postPass);
        // foreach (var feature in asset.roXamiRenderFeatures)
        // {
        //     if (feature != null)
        //     {
        //         feature.AddRenderPasses(this, ref renderingData);
        //     }
        // }
    }

    private static void SortStable(List<RoXamiRenderPass> list)
    {
        for (int i = 1; i < list.Count; ++i)
        {
            RoXamiRenderPass curr = list[i];
    
            var j = i - 1;
            for (; j >= 0 && curr < list[j]; --j)
                list[j + 1] = list[j];
    
            list[j + 1] = curr;
        }
    }
    
    public void EnqueuePass(RoXamiRenderPass pass)
    {
        activePasses.Add(pass);
    }
}