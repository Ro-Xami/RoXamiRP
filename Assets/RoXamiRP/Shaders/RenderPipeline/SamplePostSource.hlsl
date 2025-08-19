#ifndef ROXAMIRP_SAMPLE_POSTSOURCE_INCLUDE
#define ROXAMIRP_SAMPLE_POSTSOURCE_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

TEXTURE2D(_PostSource0);
TEXTURE2D(_PostSource1);
SAMPLER(sampler_linear_clamp);

float4 _PostSource0_TexelSize;
#define texelSize _PostSource0_TexelSize.xy

float4 GetSourceTexelSize()
{
    return _PostSource0_TexelSize;
}

float4 GetSource0(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostSource0, sampler_linear_clamp, screenUV, 0);
}

float4 GetSourceBicubic (float2 screenUV) {
    return SampleTexture2DBicubic(
        TEXTURE2D_ARGS(_PostSource0, sampler_linear_clamp), screenUV,
        GetSourceTexelSize().zwxy, 1.0, 0.0
    );
}

float4 GetSource1(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostSource1, sampler_linear_clamp, screenUV, 0);
}

#endif
