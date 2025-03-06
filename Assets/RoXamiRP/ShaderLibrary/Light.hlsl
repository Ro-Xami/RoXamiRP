#ifndef ROXAMIRP_LIGHT_INCLUDE
#define ROXAMIRP_LIGHT_INCLUDE

CBUFFER_START(_CustomLight)
	float3 _DirectionalLightColor;
	float3 _DirectionalLightDirection;
CBUFFER_END

struct Light
{
	float3 direction;
	float3 color;
	float shadowAttenuation;
};

Light GetMainLight()
{
	Light light;
	light.direction = _DirectionalLightDirection;
	light.color = _DirectionalLightColor;
	light.shadowAttenuation = 1;
	return light;
}

#endif