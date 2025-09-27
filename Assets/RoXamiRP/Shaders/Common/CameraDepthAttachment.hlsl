#ifndef ROXAMIRP_CAMERA_DEPTH_ATTACHMENT_INCLUDE
#define ROXAMIRP_CAMERA_DEPTH_ATTACHMENT_INCLUDE
#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

float SampleCameraDepth(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
}

float4 GetCameraDepthTexelSize()
{
    return _CameraDepthTexture_TexelSize;
}
#endif
