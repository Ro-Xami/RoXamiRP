#ifndef ROXAMIRP_SSPR_INCLUDE
#define ROXAMIRP_SSPR_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_SSPRTexture);
SAMPLER(sampler_SSPRTexture);
float4 SampleSSPRTexture(float2 screenSpaceUV)
{
    return SAMPLE_TEXTURE2D(_SSPRTexture, sampler_SSPRTexture, screenSpaceUV);
}

#endif