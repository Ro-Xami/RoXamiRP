#ifndef ACTOR_LIGHTING_INCLUDE
#define ACTOR_LIGHTING_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Light.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/GI.hlsl"
#include "Assets/RoXamiRP/Shaders/Common/ToonBRDF.hlsl"

#define _rimSmoothLeft 0.5
#define _rimSmoothRight 0.65
float3 _ActorRimColor;
float _ActorRimOffest;
float _ActorRimThreshold;

TEXTURE2D(_ActorLutMap);
SAMPLER(sampler_ActorLutMap);

TEXTURE2D(_ActorSkinMap);
SAMPLER(sampler_ActorSkinMap);

ToonBRDF GetActorLitBRDFData(Input input, Surface surface, Light light)
{
	float NoL;
	ToonBRDF brdfData = GetToonBRDF(input, surface, light, NoL);

	float clampedNoL = clamp((NoL * lerp(0.5, 1, light.shadowAttenuation) * 0.5f + 0.5f), 0.02f, 0.98f);
	brdfData.toonDiffuse = SAMPLE_TEXTURE2D(_ActorLutMap, sampler_ActorLutMap, clampedNoL).rgb;

	return brdfData;
}

//==============================================================================================================
//NPR Function
float3 DirectSpec_Hair(float NoH , float2 uv1 , float3 viewDir)
{
	// float3 specHair = lerp(_specMin , _specMax, NoH);
	// #if defined(_ISSPECMAP_ON)
	// specHair *= SAMPLE_TEXTURE2D(_SpecMap, sampler_SpecMap, uv1 + float2(0, -viewDir.y * 0.1 + 1)).rgb;
	// #endif
	// return specHair;
	return 1;
}

float DepthRim(Input inputData, float depth)
{
	float3 normalVS = TransformWorldToViewDir(inputData.normalWS, true);
	float2 signDir = normalVS.xy;
	float2 offestSamplePos = inputData.screenSpaceUV + _ActorRimOffest * GetCameraDepthTexelSize().xy / inputData.positionCS.w * signDir;
	float offsetDepth = SampleCameraDepth(offestSamplePos);
	//Rim
	float linear01EyeOffestDepth = Linear01Depth(offsetDepth , _ZBufferParams);
	float linear01EyeDepth = Linear01Depth(depth , _ZBufferParams);
	float depthDiffer = linear01EyeOffestDepth - linear01EyeDepth;
	float rim = step(_ActorRimThreshold * 0.001, depthDiffer);
	return rim;
}

//=====================================================================================
//Lighting
float3 GetDirectionalLight (Surface surface, Input input, ToonBRDF brdf, Light light)
{ 
	float3 Ks = Fresnel_Light(brdf.HoL, brdf.F0);
	float3 Kd = saturate((1 - Ks)) * (1 - surface.metallic);
    
	//BRDF
	float3 BRDFSpec = GGX_Spec(brdf.HoL , brdf.NoH , brdf.F0 , surface.roughness);

	return (Kd * brdf.toonDiffuse + BRDFSpec * brdf.NoL) * light.color;
}

float3 GetRimColor(Input inputData, float NoL, float depth)
{
	return NoL * DepthRim(inputData, depth);
}

float3 GetRimColor(Input inputData, ToonBRDF brdf, float depth)
{
	return GetRimColor(inputData, brdf.NoL, depth);
}

float3 GetInDirectionalLight(Input inputData, Surface surfaceData, ToonBRDF brdfData, Light light, GI gi)
{
	float3 SHColor = gi.diffuse;
	float3 Ks = Fresnel_InLight(brdfData.NoV, surfaceData.roughness, brdfData.F0);
	float3 Kd = saturate((1 - Ks)) * (1 - surfaceData.metallic);
	float3 InDiffuse = SHColor * Kd * surfaceData.albedo;
    
	float3 F_IndirectionLight = Ks;
	float3 SpecCubeColor = gi.specular;
	float2 LUT = LUT_Approx(surfaceData.roughness, brdfData.NoV);
	float3 InSpec = SpecCubeColor * (F_IndirectionLight * LUT.r + LUT.g);
	return (InDiffuse + InSpec) * surfaceData.ao;
}

float3 GetAdditionalLightColor(Input input)
{
	float3 additionalLightColor = float3(0 , 0 , 0);
	int additionalLightCount = GetAdditionalLightCount();
	
	UNITY_LOOP
	for (int additionLightIndex = 0 ; additionLightIndex < additionalLightCount ; additionLightIndex++)
	{
		Light additionalLight = GetAdditionalLight(additionLightIndex , input);
		float NoL = saturate(dot(additionalLight.direction , input.normalWS));
		additionalLightColor += additionalLight.color * NoL * additionalLight.shadowAttenuation;
	}
	return additionalLightColor;
}

float3 GetEmissiveColor(Surface surface)
{
	return surface.emissive;
}

//===========================================================================================================================
//FinalColor
float4 CalculateActorLighting(Input inputData , Surface surfaceData, float depth)
{
	Light mainLight = GetMainLight(inputData);
	GI gi = GetGI(inputData , surfaceData);
	ToonBRDF brdfData = GetActorLitBRDFData(inputData, surfaceData, mainLight);

	//return float4(gi.specular, 1);

	float4 color = 0;
	color.rgb += GetDirectionalLight (surfaceData, inputData, brdfData, mainLight);
	color.rgb += GetAdditionalLightColor(inputData);
	color.rgb += GetInDirectionalLight(inputData, surfaceData, brdfData, mainLight, gi);
	color.rgb *= surfaceData.albedo;
	color.rgb += GetRimColor(inputData, brdfData, depth);
	color.rgb += GetEmissiveColor(surfaceData);
	color.a = 1;
	
	return color;
}

float4 CalculateActorSkin(Input inputData , Surface surfaceData, float depth)
{
	Light mainLight = GetMainLight(inputData);
	//ToonBRDF brdfData = GetActorLitBRDFData(inputData, surfaceData, mainLight);
	GI gi = GetGI(inputData , surfaceData);

	float faceMask = 0;//(1 - surfaceData.alpha);
	float shadow = saturate(mainLight.shadowAttenuation + faceMask);
	float diffuse = dot(surfaceData.normal, mainLight.direction);
	diffuse = (diffuse * shadow + 1) * 0.5;
	diffuse = min(0.98f, max(0.02f, diffuse));
	float3 toonDiffuse = SAMPLE_TEXTURE2D(_ActorSkinMap, sampler_ActorSkinMap, float2(diffuse, 0)).rgb;

	float4 color = 0;
	color.rgb += toonDiffuse;
	color.rgb *= surfaceData.albedo;
	color.rgb += GetRimColor(inputData, diffuse, depth);
	color.a = 1;
	
	return color;
}

#endif