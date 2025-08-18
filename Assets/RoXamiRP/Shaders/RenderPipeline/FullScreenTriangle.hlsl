#ifndef ROXAMIRP_FULLSCREEN_TRIANGLE_INCLUDE
#define ROXAMIRP_FULLSCREEN_TRIANGLE_EXCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/UnityInput.hlsl"

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

#endif