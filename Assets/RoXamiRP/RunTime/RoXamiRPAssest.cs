using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "RoXamiRP/RoXamiRP Asset")]
public class RoXamiRPAssest : RenderPipelineAsset
{
    [SerializeField]
    bool SRPBatcher = true, GPUInstancing = true, DynamicBatching = false;
    protected override RenderPipeline CreatePipeline()
    {
        return new RoXamiRP(
            SRPBatcher , GPUInstancing, DynamicBatching
            );
    }
}
