using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RoXamiRPAsset", menuName = "RoXamiRP/RoXamiRP Asset")]
public class RoXamiRPAsset : RenderPipelineAsset
{
    [FormerlySerializedAs("roxamiRenderer")] [SerializeField]
    RoXamiRendererAsset roxamiRendererAsset;

    [SerializeField]
    bool isHDR = true, SRPBatcher = true, GPUInstancing = true, DynamicBatching = false;

    [SerializeField]
    ShadowSettings shadowSettings = default;

    protected override RenderPipeline CreatePipeline()
    {
        return new RoXamiRP(
            SRPBatcher , GPUInstancing, DynamicBatching, shadowSettings , roxamiRendererAsset, isHDR
            );
    }
}
