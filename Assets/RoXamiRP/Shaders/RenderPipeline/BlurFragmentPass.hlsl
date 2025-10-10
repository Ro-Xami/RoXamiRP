#ifndef ROXAMIRP_BLUR_PASS_INCLUDE
#define ROXAMIRP_BLUR_PASS_INCLUDE

#include "Assets/RoXamiRP/Shaders/RenderPipeline/SampleTempRtSource.hlsl"

float4 GaussianBlurPass(Varyings IN) : SV_Target
{
    float4 col = 0;
    col += GetSource0(IN.uv) * 0.4;
    col += GetSource0(IN.uv1.xy) * 0.15;
    col += GetSource0(IN.uv1.zw) * 0.15;
    col += GetSource0(IN.uv2.xy) * 0.10;
    col += GetSource0(IN.uv2.zw) * 0.10;
    col += GetSource0(IN.uv3.xy) * 0.05;
    col += GetSource0(IN.uv3.zw) * 0.05;
    
    return col;
}

#endif