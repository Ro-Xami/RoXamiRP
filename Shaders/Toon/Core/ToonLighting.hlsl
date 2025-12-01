#ifndef TOON_LIGHTING_INCLUDE
#define TOON_LIGHTING_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

Light GetToonDeferredMainLight(float3 posWS, float2 screenUV)
{
    Light mainLight = GetMainLight();
    mainLight.distanceAttenuation = 1.0;

    half4 shadowMask = half4(0,0,0,0);//不考虑烘焙

#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
    float4 shadowCoord = float4(screenUV, 0.0, 1.0);
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    float4 shadowCoord = TransformWorldToShadowCoord(posWS.xyz);
#else
    float4 shadowCoord = float4(0, 0, 0, 0);
#endif
    mainLight.shadowAttenuation = MainLightShadow(shadowCoord, posWS.xyz, shadowMask, _MainLightOcclusionProbes);

#if defined(_LIGHT_COOKIES)
    real3 cookieColor = SampleMainLightCookie(posWS);
    mainLight.color *= half3(cookieColor);
#endif

    return mainLight;
}

half3 LightingToonBased(BRDFData brdfData, Light light, InputData inputData)
{
    half NdotL = saturate(dot(inputData.normalWS, light.direction));
    half3 radiance = light.color * (light.shadowAttenuation * light.distanceAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;
    brdf += brdfData.specular * DirectBRDFSpecular(brdfData, inputData.normalWS, light.direction, inputData.viewDirectionWS);

    return brdf * radiance;
}

#endif