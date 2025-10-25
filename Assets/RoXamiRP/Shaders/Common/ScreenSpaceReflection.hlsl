#ifndef ROXAMIRP_SSR_INCLUDE
#define ROXAMIRP_SSR_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_ScreenSpaceReflectionTexture);
SAMPLER(sampler_ScreenSpaceReflectionTexture);

float4 SampleSSRTexture(float2 uv, float mip)
{
    return SAMPLE_TEXTURE2D_LOD(_ScreenSpaceReflectionTexture, sampler_ScreenSpaceReflectionTexture, uv, mip);
}

#endif