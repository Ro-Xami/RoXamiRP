#ifndef ROXAMIRP_FULLSCREENTRIANGLE_INCLUDE
#define ROXAMIRP_FULLSCREENTRIANGLE_INCLUDE

#include "../ShaderLibrary/UnityInput.hlsl"

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

float4 GetSource0(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostSource0, sampler_linear_clamp, screenUV, 0);
}

float4 GetSource1(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostSource1, sampler_linear_clamp, screenUV, 0);
}

float4 CopyPassFragment (Varyings IN) : SV_TARGET
{
    return GetSource0(IN.uv);
}

float4 _PostFXSource_TexelSize;

float4 GetSourceTexelSize ()
{
    return 0.001f;
    return _PostFXSource_TexelSize;
}

float4 _BloomThreshold;

float3 ApplyBloomThreshold (float3 color)
{
    float brightness = Max3(color.r, color.g, color.b);
    float soft = brightness + _BloomThreshold.y;
    soft = clamp(soft, 0.0, _BloomThreshold.z);
    soft = soft * soft * _BloomThreshold.w;
    float contribution = max(soft, brightness - _BloomThreshold.x);
    contribution /= max(brightness, 0.00001);

    return color * 2;
    return color * contribution;
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
    //return GetSource0(IN.uv) + float4(0.15,0,0,1);
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
    //return GetSource0(IN.uv) + float4(0,0.15,0,1);
    return float4(color, 1.0);
}

float4 BloomUpSamplePassFragment (Varyings IN) : SV_TARGET
{
    float4 s0 = GetSource0(IN.uv);
    float4 s1 = GetSource1(IN.uv);

    return (s0 + s1) * 0.5;
}

float4 BloomCombineFragment(Varyings IN) : SV_TARGET
{
    float4 s0 = GetSource0(IN.uv);
    float4 s1 = GetSource1(IN.uv);

    return s0 + s1;
}

#endif
