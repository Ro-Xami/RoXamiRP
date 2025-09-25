#ifndef ACTOR_LIGHTING_INCLUDE
#define ACTOR_LIGHTING_INCLUDE

#include "Assets/RoXamiRP/Shaders/Actor/ActorToonLitInput.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Light.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/GI.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/CameraAttachment.hlsl"

#define linear_F0 0.04

#define _rimSmoothLeft 0.5
#define _rimSmoothRight 0.65
#define _rimColor float3(1, 1, 1)
#define _rimOffest 3
#define _rimThreshold 0.05

TEXTURE2D(_LutMap);
SAMPLER(sampler_LutMap);

TEXTURE2D(_SdfFaceMap);
SAMPLER(sampler_SdfFaceMap);

struct LightingData
{
    half3 giColor;
    half3 mainLightColor;
    half3 additionalLightsColor;
    half3 emissionColor;
};

struct CommonData
{
    float3 F0;
    float3 halfDir;
    float NoH;
    float NoL;
    float NoV;
    float HoV;
    float HoL;
};

CommonData GetCommonData(Light light, Surface surface, Input input)
{
    CommonData data = (CommonData) 0;

    data.F0 = lerp(linear_F0, surface.albedo, surface.metallic);
    data.halfDir = SafeNormalize(input.viewWS + light.direction);
    data.NoH = saturate(dot(surface.normal, data.halfDir));
    data.NoL = saturate(dot(surface.normal, light.direction));
    data.NoV = saturate(dot(surface.normal, input.viewWS));
    data.HoV = saturate(dot(input.viewWS, light.direction));
    data.HoL = saturate(dot(data.halfDir, light.direction));

    return data;
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

float SDF_NoL(float2 uv, float3 lightDir)
{
	float4 leftLightShadow = SAMPLE_TEXTURE2D(_SdfFaceMap, sampler_SdfFaceMap, uv);
	float4 rightLightShadow = SAMPLE_TEXTURE2D(_SdfFaceMap, sampler_SdfFaceMap, float2(1 - uv.x , uv.y));
	float2 rightDir_XZ = normalize(half2(1,0));
	float2 lightDir_XZ = normalize(lightDir.xz);
	float2 frontDir_XZ = normalize(_faceFrontDir.xz);
	float isFront = dot(lightDir_XZ , frontDir_XZ);
	float isRight = dot(lightDir.xz , rightDir_XZ);
	float4 sdf_LightShadow = isRight > 0 ? rightLightShadow : leftLightShadow;
	float NoL = (sdf_LightShadow.r - 0.5) * 2 + isFront;
	NoL = min(0.95, NoL);
	NoL = max(0.05, NoL);

	return NoL;
}

float DepthRim(float2 screenSpaceUV , float3 normal , float positionCS_W)
{
	float3 normalVS = TransformWorldToViewDir(normal, true);
	float2 signDir = normalVS.xy;
	float2 offestSamplePos = screenSpaceUV + _rimOffest * GetCameraDepthTexelSize().xy / positionCS_W * signDir;
	float offsetDepth = SampleCameraDepth(offestSamplePos);
	float depth = SampleCameraDepth(screenSpaceUV);
	//Rim
	float linear01EyeOffestDepth = Linear01Depth(offsetDepth , _ZBufferParams);
	float linear01EyeDepth = Linear01Depth(depth , _ZBufferParams);
	float depthDiffer = linear01EyeOffestDepth - linear01EyeDepth;
	float rim = step(_rimThreshold * 0.001, depthDiffer);
	return rim;
}

//===========================================================================================================================
//FinalColor
float4 CalculateActorLighting(Input inputData , Surface surfaceData)
{
	Light mainLight = GetMainLight(inputData);
	GI gi = GetGI(inputData , surfaceData);

    CommonData commonData = GetCommonData(mainLight, surfaceData, inputData);

	float diffuse = dot(surfaceData.normal, mainLight.direction);
	diffuse = (diffuse + 1) * 0.5;
	diffuse = lerp(0.5f, diffuse, mainLight.shadowAttenuation);
	
	float3 toonDiffuse = SAMPLE_TEXTURE2D(_LutMap, sampler_LutMap, float2(diffuse, 0)).rgb;
	float3 toonRim = smoothstep(_rimSmoothLeft, _rimSmoothRight, diffuse) *
		DepthRim(inputData.screenSpaceUV, surfaceData.normal, inputData.positionCS.w);
	
	float3 mainLightColor = surfaceData.albedo * toonDiffuse + toonRim;

	return float4(mainLightColor, 1);
	
	float3 additionalLightColor = float3(0 , 0 , 0);
	int additionalLightCount = GetAdditionalLightCount();
	
	UNITY_LOOP
	for (int additionLightIndex = 0 ; additionLightIndex < additionalLightCount ; additionLightIndex++)
	{
		Light additionalLight = GetAdditionalLight(additionLightIndex , inputData);
		float NoL = saturate(dot(additionalLight.direction , surfaceData.normal));
		additionalLightColor += additionalLight.color * NoL * additionalLight.shadowAttenuation;
	}

    float4 finalColor = float4(0, 0, 0, 0);

    finalColor.a = surfaceData.alpha;

    return finalColor;
}

float4 CalculateActorFace(Input inputData , Surface surfaceData, float2 uv)
{
	Light mainLight = GetMainLight(inputData);
	GI gi = GetGI(inputData , surfaceData);

	CommonData commonData = GetCommonData(mainLight, surfaceData, inputData);

	float diffuse = dot(surfaceData.normal, mainLight.direction);
	diffuse = (diffuse + 1) * 0.5;

	diffuse = SDF_NoL(uv, mainLight.direction);

	//return diffuse;
	
	float3 toonDiffuse = SAMPLE_TEXTURE2D(_LutMap, sampler_LutMap, float2(diffuse, 0)).rgb;
	float3 toonRim = smoothstep(_rimSmoothLeft, _rimSmoothRight, diffuse) *
		DepthRim(inputData.screenSpaceUV, surfaceData.normal, inputData.positionCS.w);
	
	float3 mainLightColor = surfaceData.albedo * toonDiffuse + toonRim;

	return float4(mainLightColor, surfaceData.alpha);

	
	float3 additionalLightColor = float3(0 , 0 , 0);
	int additionalLightCount = GetAdditionalLightCount();
	
	UNITY_LOOP
	for (int additionLightIndex = 0 ; additionLightIndex < additionalLightCount ; additionLightIndex++)
	{
		Light additionalLight = GetAdditionalLight(additionLightIndex , inputData);
		float NoL = saturate(dot(additionalLight.direction , surfaceData.normal));
		additionalLightColor += additionalLight.color * NoL * additionalLight.shadowAttenuation;
	}

	float4 finalColor = float4(0, 0, 0, 0);

	finalColor.a = surfaceData.alpha;

	return finalColor;
}

#endif