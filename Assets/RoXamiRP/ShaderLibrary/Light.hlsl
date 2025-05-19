#ifndef ROXAMIRP_LIGHT_INCLUDE
#define ROXAMIRP_LIGHT_INCLUDE

#include "Shadows.hlsl"

CBUFFER_START(_RoXamiLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColor;
	float4 _DirectionalLightDirection;
	
CBUFFER_END

struct Light
{
	float3 direction;
	float3 color;
	float shadowAttenuation;
};

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

Light GetMainLight(float3 positionWS , float3 normalWS)
{
	Light light;

    light.direction = normalize(_DirectionalLightDirection.xyz);
    light.color = _DirectionalLightColor.xyz;
	light.shadowAttenuation = GetDirectionalShadowAttenuation(positionWS , normalWS);
	return light;
}

#endif