#ifndef ROXAMIRP_FULLSCREENTRIANGLE_INCLUDE
#define ROXAMIRP_FULLSCREENTRIANGLE_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/UnityInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

struct Varyings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCORRD0;
};

Varyings FullScreenTriangle (uint vertexID : SV_VertexID) {
    Varyings OUT = (Varyings)0;
    OUT.positionCS = float4(
        vertexID <= 1 ? -1.0 : 3.0,
        vertexID == 1 ? 3.0 : -1.0,
        0.0, 1.0
    );
    OUT.uv = float2(
        vertexID <= 1 ? 0.0 : 2.0,
        vertexID == 1 ? 2.0 : 0.0
    );
    if (_ProjectionParams.x < 0.0) {
        OUT.uv.y = 1.0 - OUT.uv.y;
    }
    return OUT;
}

TEXTURE2D(_PostSource0);
TEXTURE2D(_PostSource1);
SAMPLER(sampler_linear_clamp);
float4 _PostSource0_TexelSize;

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

float4 CopyPassFragment (Varyings IN) : SV_TARGET
{
    return GetSource0(IN.uv);
}

//================================================Bloom====================================================
float4 _bloomParam;
#define threshold _bloomParam.x
#define thresholdKnee _bloomParam.y
#define clampMax _bloomParam.z
#define scatter _bloomParam.w
float _bloomIntensity;

float3 ApplyBloomThreshold (float3 color)
{
    // User controlled clamp to limit crazy high broken spec
    color = min(clampMax, color);

    // Thresholding,soft the threshold
    half brightness = Max3(color.r, color.g, color.b);
    half softness = clamp(brightness - threshold + thresholdKnee, 0.0, 2.0 * thresholdKnee);
    softness = (softness * softness) / (4.0 * thresholdKnee + 1e-4);
    half multiplier = max(brightness - threshold, softness) / max(brightness, 1e-4);
    color *= multiplier;

    // Clamp colors to positive once in prefilter. Encode can have a sqrt, and sqrt(-x) == NaN. Up/Downsample passes would then spread the NaN.
    color = max(color, 0);

    return color;
    //return EncodeHDR(color);
}

float4 BloomPrefilterPassFragment (Varyings IN) : SV_TARGET
{
    float3 color = ApplyBloomThreshold(GetSource0(IN.uv).rgb);
    return float4(color, 1.0);
}

float4 BloomHorizontalPassFragment (Varyings IN) : SV_TARGET
{
    float3 color = 0.0;
    float offsets[] = {
        -3.23076923, -1.38461538, 0.0, 1.38461538, 3.23076923
    };
    float weights[] = {
        0.07027027, 0.31621622, 0.22702703, 0.31621622, 0.07027027
    };
    for (int i = 0; i < 5; i++) {
        float offset = offsets[i] * 2.0 * GetSourceTexelSize().x;
        color += GetSource0(IN.uv + float2(offset, 0.0)).rgb * weights[i];
    }
    return float4(color, 1.0);
}

float4 BloomVerticalPassFragment (Varyings IN) : SV_TARGET
{
    float3 color = 0.0;
    float offsets[] = {
        -4.0, -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0
    };
    float weights[] = {
        0.01621622, 0.05405405, 0.12162162, 0.19459459, 0.22702703,
        0.19459459, 0.12162162, 0.05405405, 0.01621622
    };
    for (int i = 0; i < 9; i++) {
        float offset = offsets[i] * GetSourceTexelSize().y;
        color += GetSource0(IN.uv + float2(0.0, offset)).rgb * weights[i];
    }
    return float4(color, 1.0);
}

float4 BloomUpSamplePassFragment (Varyings IN) : SV_TARGET
{
    float4 low = GetSourceBicubic(IN.uv);
    float4 high = GetSource1(IN.uv);

    return lerp(high , low , scatter);
}

float4 BloomCombineFragment(Varyings IN) : SV_TARGET
{
    float4 s0 = GetSource0(IN.uv);
    float4 s1 = GetSource1(IN.uv);

    //return s1 * _bloomIntensity;
    return s0 + s1 * _bloomIntensity;
}

#endif
