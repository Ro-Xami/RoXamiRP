#ifndef ROXAMIRP_CAMERAATTCHMENT_INCLUDE
#define ROXAMIRP_CAMERAATTCHMENT_INCLUDE
#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
float SampleCameraDepth(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
}

TEXTURE2D(_CameraColorTexture);
SAMPLER(sampler_CameraColorTexture);
float3 SampleCameraColor(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uv).rgb;
}

TEXTURE2D(_WorldSpacePositionTexture);
SAMPLER(sampler_WorldSpacePositionTexture);
float3 SampleWorldSpacePosition(float2 uv)
{
    return SAMPLE_TEXTURE2D(_WorldSpacePositionTexture, sampler_WorldSpacePositionTexture, uv).rgb;
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

float3 CalculateDepthToPositionWS(float reverseZ, float2 screenUV)
{
    float4 ndc = float4(screenUV * 2.0 - 1.0, reverseZ, 1.0);

    if (!_ProjectionParams.x)
    {
        ndc.y = -ndc.y;
    }

    float4 positionWS = mul(MATRIX_I_VP, ndc);
    return positionWS.xyz / positionWS.w;
}

#endif
