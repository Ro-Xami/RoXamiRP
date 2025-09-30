#ifndef ROXAMIRP_FULLSCREEN_TRIANGLE_INCLUDE
#define ROXAMIRP_FULLSCREEN_TRIANGLE_EXCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/UnityInput.hlsl"

struct Varyings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCORRD0;
    
#ifdef _Post_Gaussian_BlurPass
    float4 uv1 : TEXCOORD1;
    float4 uv2 : TEXCOORD2;
    float4 uv3 : TEXCOORD3;
#endif
};

#ifdef _Post_Gaussian_BlurPass
    float4 _Post_GaussianBlurOffset;
#endif

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

#ifdef _Post_Gaussian_BlurPass
    OUT.uv1 = OUT.uv.xyxy + float4(1, 1, -1, -1) * _Post_GaussianBlurOffset.xyxy;
    OUT.uv2 = OUT.uv.xyxy + float4(1, 1, -1, -1) * _Post_GaussianBlurOffset.xyxy * 2;
    OUT.uv3 = OUT.uv.xyxy + float4(1, 1, -1, -1) * _Post_GaussianBlurOffset.xyxy * 6;
#endif
    return OUT;
}

#endif