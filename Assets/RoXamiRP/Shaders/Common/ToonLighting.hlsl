#ifndef ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#define ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#include "Assets//RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/Light.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/GI.hlsl"
#include "Assets/RoXamiRP/Shaders/Common/ToonBRDF.hlsl"


float3 GetDirectionalLight (Surface surface, Input input, ToonBRDF brdf, Light light)
{ 
    float3 Ks = Fresnel_Light(brdf.HoL, brdf.F0);
    float3 Kd = saturate((1 - Ks)) * (1 - surface.metallic);
    
    //BRDF
    float3 BRDFSpec = GGX_Spec(brdf.HoL , brdf.NoH , brdf.F0 , surface.roughness);

    return (Kd * brdf.toonDiffuse + BRDFSpec * brdf.NoL) * light.color;
}

float3 InDirectionalLight(float NoV , float3 normalWS, float3 viewWS , float3 albedo , float metallic , float roughness, float occlusion, float3 F0 , GI gi)
{
    float3 SHColor = gi.diffuse;
    float3 Ks = Fresnel_InLight(NoV, roughness, F0);
    float3 Kd = saturate((1 - Ks)) * (1 - metallic);
    float3 InDiffuse = SHColor * Kd * albedo;
    
    float3 F_IndirectionLight = Ks;
    float3 SpecCubeColor = gi.specular;
    float2 LUT = LUT_Approx(roughness, NoV);
    float3 InSpec = SpecCubeColor * (F_IndirectionLight * LUT.r + LUT.g);
    return (InDiffuse + InSpec) * occlusion;
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
    ToonBRDF brdfData = GetToonBRDF(inputData, surfaceData, mainLight);
    
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
    ToonBRDF brdfData = GetToonBRDF(inputData, surfaceData, mainLight);
    
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
    float3 albedo = surfaceData.albedo;
    float3 normalDir = surfaceData.normal;
    float occlusion = surfaceData.ao;
    float roughness = LinearStep( 0.003 , 1 , surfaceData.roughness);
    float metallic = surfaceData.metallic;
    
    GI gi = GetGI(inputData , surfaceData);

    float NoV = max(saturate(dot(normalDir, inputData.viewWS)), 0.01);

    float3 F0 = lerp(linear_F0, albedo, metallic);
    
    float4 finalColor = float4(0, 0, 0, 0);
    finalColor.rgb = InDirectionalLight(NoV, normalDir, inputData.viewWS, albedo, metallic, roughness, occlusion , F0 , gi);
    finalColor.a = 1;

    return finalColor;
}

#endif