#ifndef ROXAMIRP_CAMERAATTCHMENT_INCLUDE
#define ROXAMIRP_CAMERAATTCHMENT_INCLUDE
#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_CameraOpaqueDepthTexture);
SAMPLER(sampler_CameraOpaqueDepthTexture);

float SampleCameraOpaqueDepth(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraOpaqueDepthTexture, sampler_CameraOpaqueDepthTexture, uv).r;
}

TEXTURE2D(_CameraOpaqueColorTexture);
SAMPLER(sampler_CameraOpaqueColorTexture);

float3 SampleCameraOpaqueColor(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraOpaqueColorTexture, sampler_CameraOpaqueColorTexture, uv);
}

//=====================Function=======================

float GetReverseDepth(float depth)
{
#if defined(UNITY_REVERSED_Z)
    float reverseZ = depth;
#else
    float reverseZ = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
#endif
    
    return reverseZ;
}

// float3 CalculateDepthToPositionWS(float reverseZ, float2 screenUV)
// {
//     float4 positionCS = float4(screenUV * 2.0 - 1.0, reverseZ, 1.0);
//
//     #if UNITY_UV_STARTS_AT_TOP
//     positionCS.y = -positionCS.y;
//     #endif
//     
//     float4 positionWS = mul(UNITY_MATRIX_I_VP, positionCS);
//     return positionWS.xyz / positionWS.w;
// }

#endif
