#ifndef ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#define ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#include "Assets//RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/Light.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/GI.hlsl"
#include "Assets/RoXamiRP/Shaders/Common/ToonBRDF.hlsl"
#include "Assets/RoXamiRP/Shaders/Common/SampleHBAOTexture.hlsl"

TEXTURE2D(_ToonLitLut);
SAMPLER(sampler_ToonLitLut);

ToonBRDF GetToonLitBRDFData(Input input, Surface surface, Light light)
{
    float NoL;
    ToonBRDF brdfData = GetToonBRDF(input, surface, light, NoL);

    float clampedNoL = clamp((NoL * light.shadowAttenuation * 0.5f + 0.5f), 0.05f, 0.95f);
    brdfData.toonDiffuse = SAMPLE_TEXTURE2D(_ToonLitLut, sampler_ToonLitLut, clampedNoL).rgb;

    return brdfData;
}

float3 GetDirectionalLight (Surface surface, Input input, ToonBRDF brdf, Light light)
{ 
    float3 Ks = Fresnel_Light(brdf.HoL, brdf.F0);
    float3 Kd = saturate((1 - Ks)) * (1 - surface.metallic);
    
    //BRDF
    float3 BRDFSpec = GGX_Spec(brdf.HoL , brdf.NoH , brdf.F0 , surface.roughness);

    return (Kd * brdf.toonDiffuse + BRDFSpec * brdf.NoL) * light.color;
}

float3 InDirectionalLight(Input inputData, Surface surfaceData, ToonBRDF brdfData, Light light, GI gi)
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
    //return InSpec;
}

float3 GetAdditionalLightColor(Input input, Surface surface)
{
    float3 additionalLightColor = float3(0 , 0 , 0);
    int additionalLightCount = GetAdditionalLightCount();
	
    UNITY_LOOP
    for (int additionLightIndex = 0 ; additionLightIndex < additionalLightCount ; additionLightIndex++)
    {
        Light additionalLight = GetAdditionalLight(additionLightIndex , input);
        float NoL = saturate(dot(additionalLight.direction , surface.normal));
        additionalLightColor += additionalLight.color * NoL * additionalLight.shadowAttenuation;
    }
    return additionalLightColor;
}

float3 GetEmissiveColor(Surface surface)
{
    return surface.emissive;
}

float GetTransparentAlpha(Surface surface)
{
    return surface.alpha;
}

//===========================================================================================================================
float4 CalculateToonLighting(Input inputData , Surface surfaceData)
{
    Light mainLight = GetMainLight(inputData);
    ToonBRDF brdfData = GetToonLitBRDFData(inputData, surfaceData, mainLight);
    
    float4 finalColor = float4(0, 0, 0, 0);
    finalColor.rgb += GetDirectionalLight(surfaceData, inputData, brdfData, mainLight);
    finalColor.rgb += GetAdditionalLightColor(inputData , surfaceData);
    finalColor.rgb *= surfaceData.albedo;
    finalColor.rgb += GetEmissiveColor(surfaceData);
    finalColor.a = GetTransparentAlpha(surfaceData);

    return finalColor;
}

float4 CalculateDeferredToonLitDiffuseEmission(Input inputData , Surface surfaceData)
{
    Light mainLight = GetMainLight(inputData);
    ToonBRDF brdfData = GetToonLitBRDFData(inputData, surfaceData, mainLight);
    
    float4 finalColor = float4(0, 0, 0, 0);
    finalColor.rgb += GetDirectionalLight(surfaceData, inputData, brdfData, mainLight);
    finalColor.rgb += GetAdditionalLightColor(inputData , surfaceData);
    finalColor.rgb *= surfaceData.albedo;
    finalColor.rgb += GetEmissiveColor(surfaceData);
    finalColor.a = 1;

    //finalColor.rgb = saturate(dot(mainLight.direction, surfaceData.normal) * mainLight.color * mainLight.shadowAttenuation) * surfaceData.albedo;

    return finalColor;
}

float4 CalculateDeferredToonLitGI(Input inputData , Surface surfaceData)
{
    Light mainLight = GetMainLight(inputData);
    ToonBRDF brdfData = GetToonLitBRDFData(inputData, surfaceData, mainLight);
    GI gi = GetGI(inputData , surfaceData);

#if defined(HORIZONBASED_AO)
    surfaceData.ao *= saturate(1 - SampleHBAOTexture(inputData.screenSpaceUV));
#endif
    
    float4 finalColor = float4(0, 0, 0, 0);
    finalColor.rgb = InDirectionalLight(inputData , surfaceData, brdfData, mainLight, gi);
    finalColor.a = 1;

    return finalColor;
}

#endif