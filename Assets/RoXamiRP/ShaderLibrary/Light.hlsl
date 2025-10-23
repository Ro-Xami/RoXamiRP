#ifndef ROXAMIRP_LIGHT_INCLUDE
#define ROXAMIRP_LIGHT_INCLUDE

#include "Assets/RoXamiRP/Shaders/Common/ScreenSpaceShadows.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Input.hlsl"

#define _MaxAdditionalLightCount 64

CBUFFER_START(RoXamiLight)
	float4 _DirectionalLightColor;
	float4 _DirectionalLightDirection;
	int _AdditionalLightCount;
	float4 _AdditionalLightPosition[_MaxAdditionalLightCount];
	float4 _AdditionalLightColor[_MaxAdditionalLightCount];
	float4 _AdditionalLightDirection[_MaxAdditionalLightCount];
	float4 _AdditionalLightAngles[_MaxAdditionalLightCount];
CBUFFER_END

struct Light
{
	float3 direction;
	float3 color;
	float shadowAttenuation;
};

Light GetMainLight()
{
	Light light = (Light)0;

    light.direction = normalize(_DirectionalLightDirection.xyz);
    light.color = _DirectionalLightColor.xyz;

	light.shadowAttenuation = 1;

	return light;
}

Light GetMainLight(Input inputData)
{
	Light light = (Light)0;

    light.direction = normalize(_DirectionalLightDirection.xyz);
    light.color = _DirectionalLightColor.xyz;
#if defined(SCREENSPACE_SHADOWS)
	light.shadowAttenuation = SampleScreenSpaceShadows(inputData.screenSpaceUV);
#else
	light.shadowAttenuation = 1;
#endif

	return light;
}

Light GetAdditionalLight(int index , Input inputData)
{
	Light light = (Light)0;

	float3 ray = _AdditionalLightPosition[index].xyz - inputData.positionWS;
	float3 direction = normalize(ray);

	float distanceSqr = max(dot(ray, ray), 0.00001);
	float rangeAttenuation = Square(saturate(
		1.0 - Square(distanceSqr * _AdditionalLightPosition[index].w)));
	float4 spotAngles = _AdditionalLightAngles[index];
	float spotAttenuation = saturate(
		dot(direction , _AdditionalLightDirection[index].xyz) *
		spotAngles.x + spotAngles.y);
	float attenuation = spotAttenuation * rangeAttenuation / distanceSqr;

	light.direction = direction;
	light.color = _AdditionalLightColor[index].rgb;
	light.shadowAttenuation = attenuation;

	return light;
}

int GetAdditionalLightCount()
{
	return _AdditionalLightCount;
}

#endif