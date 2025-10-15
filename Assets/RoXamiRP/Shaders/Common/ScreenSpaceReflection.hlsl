#ifndef ROXAMIRP_SSR_INCLUDE
#define ROXAMIRP_SSR_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_ScreenSpaceReflectionTexture);
SAMPLER(sampler_ScreenSpaceReflectionTexture);

float4 SampleSSRTexture(float2 uv)
{
    return SAMPLE_TEXTURE2D(_ScreenSpaceReflectionTexture, sampler_ScreenSpaceReflectionTexture, uv);
}

#endif