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

TEXTURE2D(_PostSource);
SAMPLER(sampler_linear_clamp);

float4 GetSource(float2 screenUV) {
    return SAMPLE_TEXTURE2D_LOD(_PostSource, sampler_linear_clamp, screenUV, 0);
}

float4 CopyPassFragment (Varyings IN) : SV_TARGET {
    return GetSource(IN.uv);
}

#endif
