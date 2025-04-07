#ifndef ROXAMIRP_LIGHT_INCLUDE
#define ROXAMIRP_LIGHT_INCLUDE

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

#include "Shadows.hlsl"

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

Light GetMainLight(int index , float3 positionWS , float3 normalWS)
{
	Light light;
	ShadowData shadowData = GetShadowData(positionWS);
	DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);

    light.direction = _DirectionalLightDirection[index].xyz;
    light.color = _DirectionalLightColor[index].xyz;
	light.shadowAttenuation = GetDirectionalShadowAttenuation(dirShadowData , shadowData , positionWS , normalWS);
	//light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, positionWS);
	return light;
}



#endif