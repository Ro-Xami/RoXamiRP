using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "RoXamiRP/RoXamiRP Asset")]
public class RoXamiRPAssest : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new RoXamiRP();
    }
}
