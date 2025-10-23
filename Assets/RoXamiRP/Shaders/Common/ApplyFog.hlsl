#ifndef ROXAMIRP_APPLY_FOG_INCLUDE
#define ROXAMIRP_APPLY_FOG_INCLUDE

#include "Assets/RoXamiRP/Shaders/Common/CameraDepthAttachment.hlsl"

float4 _GlobalFogParams;
float3 _GlobalFogColor;
#define _fogStart _GlobalFogParams.x
#define _fogEnd  _GlobalFogParams.y
#define _fogDensity _GlobalFogParams.z

float ComputeFogLinear(float z , float start , float end)
{
    //factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
    return saturate((start - z) / (start - end));
}

float ComputeFogEXP(float z , float density)
{
    //factor = exp(-density*z)
    return 1 - exp(-density * z);
}

float ComputeFogEXP2(float z , float density)
{
    //factor = exp(-(density*z)^2)
    float fogFactor = dot(density * z , density * z);
    return 1 - exp(- fogFactor);
}

float ComputeFogIntensity(float z)
{
    float fog = 0;
    #if defined(_GlobalFog_Linear)
    fog = ComputeFogLinear(z, _fogStart, _fogEnd);
    #elif defined(_GlobalFog_EXP)
    fog = ComputeFogEXP(z, _fogDensity);
    # elif defined(_GlobalFog_EXP2)
    fog = ComputeFogEXP2(z, _fogDensity);
    #endif
    
    return saturate(fog);
}

float ComputeFogIntensityWithUV(float2 screenspaceUV)
{
    float depth = SampleCameraDepth(screenspaceUV);
    float z = LinearEyeDepth(depth, _ZBufferParams);
    float fog = ComputeFogIntensity(z);
    return fog;
}

void ApplyFog(float z, inout float3 color)
{
    #if defined(_GlobalFog_Linear) || defined(_GlobalFog_EXP) || defined(_GlobalFog_EXP2)
    float fog = ComputeFogIntensity(z);
    color = lerp(color, _GlobalFogColor, fog);
    #endif
}

void ApplyFog(float2 screenspaceUV, inout float3 color)
{
    float depth = SampleCameraDepth(screenspaceUV);
    float z = LinearEyeDepth(GetReverseDepth(depth), _ZBufferParams);
    ApplyFog(z, color);
}



#endif