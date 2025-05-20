#ifndef ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#define ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#include "Surface.hlsl"
#include "Input.hlsl"
#include "Light.hlsl"
#include "Shadows.hlsl"
#include "GI.hlsl"

float4 CalculateToonLighting(Input inputData , Surface surfaceData)
{
	Light light = GetMainLight(inputData);
	GI gi = GetGI(inputData , surfaceData);

	float4 Debug = half4(gi.specular.rgb , 1);

	return Debug;
}

#endif