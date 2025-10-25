#ifndef ROXAMIRP_GI_INCLUDE
#define ROXAMIRP_GI_INCLUDE
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/Shaders/Common/ScreenSpaceReflection.hlsl"

// TEXTURECUBE(unity_SpecCube0);
// SAMPLER(samplerunity_SpecCube0);

TEXTURECUBE(_RoXamiRpReflectionTexture);
SAMPLER(sampler_RoXamiRpReflectionTexture);
float4 _RoXamiRpReflectionTexture_HDR;

float4 _RoXamiRP_SHAr;
float4 _RoXamiRP_SHAg;
float4 _RoXamiRP_SHAb;
float4 _RoXamiRP_SHBr;
float4 _RoXamiRP_SHBg;
float4 _RoXamiRP_SHBb;
float4 _RoXamiRP_SHC;

struct GI
{
	float3 diffuse;
	float3 specular;
};

float3 SampleLightProbe (float3 normal) 
{
	float4 coefficients[7];
	coefficients[0] = _RoXamiRP_SHAr;
	coefficients[1] = _RoXamiRP_SHAg;
	coefficients[2] = _RoXamiRP_SHAb;
	coefficients[3] = _RoXamiRP_SHBr;
	coefficients[4] = _RoXamiRP_SHBg;
	coefficients[5] = _RoXamiRP_SHBb;
	coefficients[6] = _RoXamiRP_SHC;
	return max(0.0, SampleSH9(coefficients, normal));
}

float3 SampleEnvironmentCube(float3 normalWS , float3 viewWS , float mip) {
	float3 uvw = reflect(-viewWS, normalWS);
	float4 environment = SAMPLE_TEXTURECUBE_LOD(
		_RoXamiRpReflectionTexture, sampler_RoXamiRpReflectionTexture, uvw, mip
	);
	
	return DecodeHDREnvironment(environment, _RoXamiRpReflectionTexture_HDR);
}

GI GetGI(Input inputData , Surface surfaceData)
{
	GI OUT = (GI)0;
	OUT.diffuse = SampleLightProbe(inputData.normalWS);
	float mip = PerceptualRoughnessToMipmapLevel(surfaceData.roughness);
	#ifdef SCREENSPACE_REFLECTION
		float4 ssr = SampleSSRTexture(inputData.screenSpaceUV, mip);
		float3 cube = SampleEnvironmentCube(inputData.normalWS , inputData.viewWS , mip);
		OUT.specular = lerp(cube, ssr.rgb, ssr.a);
	#else
		OUT.specular = SampleEnvironmentCube(inputData.normalWS , inputData.viewWS , mip);
	#endif

	return OUT;
}

#endif