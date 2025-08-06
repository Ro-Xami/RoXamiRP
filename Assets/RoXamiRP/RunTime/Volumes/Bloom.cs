using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoXamiRenderPipeline
{
    [Serializable, VolumeComponentMenuForRenderPipeline("PostProcessing/Bloom", typeof(RoXamiRP))]

    public sealed class Bloom : VolumeComponent, IPostProcessComponent
    {
        public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);
        public MinFloatParameter clampMax = new MinFloatParameter(5f, 0f);
        public MinFloatParameter threshold = new MinFloatParameter(0.9f, 0f);
        public MinFloatParameter scatter = new MinFloatParameter(0.7f, 0f);
        public ClampedIntParameter maxSampleCount = new ClampedIntParameter(5, 0, 10);

        public bool IsActive() => intensity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}