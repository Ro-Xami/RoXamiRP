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

	float3 additionalLightColor = float3(0 , 0 , 0);
	int additionalLightCount = GetAdditionalLightCount();
	
	UNITY_LOOP
	for (int additionLightIndex = 0 ; additionLightIndex < additionalLightCount ; additionLightIndex++)
	{
		Light additionalLight = GetAdditionalLight(additionLightIndex , inputData);
		float NoL = saturate(dot(additionalLight.direction , inputData.normalWS));
		additionalLightColor += additionalLight.color * NoL * additionalLight.shadowAttenuation;
	}
	

	float4 Debug = half4(additionalLightColor , 1);

	return Debug;
}

#endif