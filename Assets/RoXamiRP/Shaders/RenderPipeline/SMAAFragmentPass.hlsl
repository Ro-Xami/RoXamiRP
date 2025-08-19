#ifndef ROXAMIRP_SMAA_FRAGMENT_PASS_INCLUDE
#define ROXAMIRP_SMAA_FRAGMENT_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/Shaders/RenderPipeline/SamplePostSource.hlsl"

float3 SampleCameraAttachment(float2 uv)
{
    return SAMPLE_TEXTURE2D_LOD(_PostSource0, sampler_linear_clamp, uv, 0).rgb;
}

float Luma(float2 uv)
{
    return GetLuma(SampleCameraAttachment(uv).rgb);
}

#define _edgeThreshold 0.05f
float4 SMAA_Edge(Varyings IN) : SV_Target
{
    float2 uv = IN.uv.xy;
    float M = Luma(uv);
    float N =   abs(Luma(uv + float2(0  , 1 ) * texelSize) - M);
    float S =   abs(Luma(uv + float2(0  , -1) * texelSize) - M);
    float W =   abs(Luma(uv + float2(-1 , 0 ) * texelSize) - M);
    float E =   abs(Luma(uv + float2(1  , 0 ) * texelSize) - M);
    float wW =  abs(Luma(uv + float2(-2 , 1 ) * texelSize) - M);
    float sS =  abs(Luma(uv + float2(0  , -2) * texelSize) - M);

    float roundMax = max(max(N, S), max(E, W));
    
    bool isW = W > _edgeThreshold;
    isW = isW && W > max(roundMax, wW) * 0.5f;

    bool isS = S > _edgeThreshold;
    isS = isS && S > max(roundMax, sS) * 0.5f;

    float2 edge = 0;
    edge.x = isW ? 1 : 0;
    edge.y = isS ? 1 : 0;
    
    return float4(edge, 0, 1);
}

float4 SMAA_Factor(Varyings IN) : SV_Target
{
    return 1;
}

float4 SMAA_Blend(Varyings IN) : SV_Target
{
    return 1;
}

#endif