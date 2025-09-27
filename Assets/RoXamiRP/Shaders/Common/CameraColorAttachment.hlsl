#ifndef ROXAMIRP_CAMERA_COLOR_ATTACHMENT_INCLUDE
#define ROXAMIRP_CAMERA_COLOR_ATTACHMENT_INCLUDE
#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_CameraColorTexture);
SAMPLER(sampler_CameraColorTexture);
float4 _CameraColorTexture_TexelSize;

float3 SampleCameraColor(float2 uv)
{
    return SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uv).rgb;
}

float4 GetCameraColorTexelSize()
{
    return _CameraColorTexture_TexelSize;
}
#endif
