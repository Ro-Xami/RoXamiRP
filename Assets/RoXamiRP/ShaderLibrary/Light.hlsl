#ifndef ROXAMIRP_LIGHT_INCLUDE
#define ROXAMIRP_LIGHT_INCLUDE

#include "Shadows.hlsl"

CBUFFER_START(RoXamiLight)
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

Light GetMainLight()
{
	Light light;

    light.direction = normalize(_DirectionalLightDirection.xyz);
    light.color = _DirectionalLightColor.xyz;

	light.shadowAttenuation = 1;

	return light;
}

Light GetMainLight(Input inputData)
{
	Light light;

    light.direction = normalize(_DirectionalLightDirection.xyz);
    light.color = _DirectionalLightColor.xyz;

	light.shadowAttenuation = 
		GetDirectionalShadowAttenuation(
		inputData.positionWS , inputData.normalWS);

	return light;
}

#endif