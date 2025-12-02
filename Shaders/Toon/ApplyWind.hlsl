#ifndef ROXAMIRP_APPLY_WIND_INCLUDE
#define ROXAMIRP_APPLY_WIND_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

float3 _globalWindDirection;

float4 _globalWindParams;
#define _windStrength _globalWindParams.x
#define _windSpeed _globalWindParams.y
#define _windNoise _globalWindParams.z

void ApplyGlobalWind(inout float3 positionWS, float weight)
{
    //float4x4 v2wMatrix = GetObjectToWorldMatrix();
    float windNoise = _windNoise * (positionWS.x + positionWS.y + positionWS.z);
    float wind = sin(_Time.x * _windSpeed + windNoise) + _windStrength;

    positionWS += wind * _globalWindDirection * weight;
}

#endif