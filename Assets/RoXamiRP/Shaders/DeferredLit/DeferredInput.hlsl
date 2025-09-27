#ifndef ROXAMIRP_DEFERRED_INPUT
#define ROXAMIRP_DEFERRED_INPUT

#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/Shaders/Common/CameraDepthAttachment.hlsl"

TEXTURE2D(_GBuffer0);
SAMPLER(sampler_GBuffer0);
TEXTURE2D(_GBuffer1);
SAMPLER(sampler_GBuffer1);
TEXTURE2D(_GBuffer2);
SAMPLER(sampler_GBuffer2);
TEXTURE2D(_GBuffer3);
SAMPLER(sampler_GBuffer3);

float4 SampleAlbedo(float2 uv)
{
    return  SAMPLE_TEXTURE2D(_GBuffer0, sampler_GBuffer0, uv);
}

float4 SampleNormal(float2 uv)
{
    return  SAMPLE_TEXTURE2D(_GBuffer1, sampler_GBuffer1, uv);
}

float4 SampleMRA(float2 uv)
{
    return  SAMPLE_TEXTURE2D(_GBuffer2, sampler_GBuffer2, uv);
}

float4 SamplerEmission(float2 uv)
{
    return  SAMPLE_TEXTURE2D(_GBuffer3, sampler_GBuffer3, uv);
}

Input GetInputData(Varyings IN, Surface surface, out float depth)
{
    Input OUT = (Input)0;
    depth = SampleCameraDepth(IN.uv);
    depth = GetReverseDepth(depth);
				
    OUT.positionWS = CalculateDepthToPositionWS(depth, IN.uv);
    OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
    OUT.normalWS = surface.normal;
    OUT.viewWS = GetViewDirWS(OUT.positionWS);
    OUT.screenSpaceUV = IN.uv;

    return OUT;
}

#endif