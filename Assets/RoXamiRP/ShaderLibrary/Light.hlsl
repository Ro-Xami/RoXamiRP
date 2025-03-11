#ifndef ROXAMIRP_LIGHT_INCLUDE
#define ROXAMIRP_LIGHT_INCLUDE

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColor[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirection[MAX_DIRECTIONAL_LIGHT_COUNT];
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

Light GetMainLight(int index)
{
	Light light;
    light.direction = _DirectionalLightDirection[MAX_DIRECTIONAL_LIGHT_COUNT];
    light.color = _DirectionalLightColor[MAX_DIRECTIONAL_LIGHT_COUNT];
	light.shadowAttenuation = 1;
	return light;
}

#endif