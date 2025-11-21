#ifndef SAMPLE_HBAO_TEXTURE_INCLUDE
#define SAMPLE_HBAO_TEXTURE_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_HBAoTexture);
SAMPLER(sampler_HBAoTexture);

float SampleHBAOTexture(float2 uv)
{
    return SAMPLE_TEXTURE2D(_HBAoTexture, sampler_HBAoTexture, uv).r;
}

#endif