#ifndef _TOON_DEFERRED_FRAGMENT
#define _TOON_DEFERRED_FRAGMENT

#include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/Utils/StencilDeferred.hlsl"

half3 LightingToonBased(BRDFData brdfData, Light light, InputData inputData)
{
    half NdotL = saturate(dot(inputData.normalWS, light.direction));
    half3 radiance = light.color * (light.shadowAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;
    brdf += brdfData.specular * DirectBRDFSpecular(brdfData, inputData.normalWS, light.direction, inputData.viewDirectionWS);

    return brdf * radiance;
}

half4 ToonDeferredShading(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 screen_uv = (input.screenUV.xy / input.screenUV.z);

#if defined(SUPPORTS_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
    float2 undistorted_screen_uv = screen_uv;
    UNITY_BRANCH if (_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
    {
        screen_uv = input.positionCS.xy * _ScreenSize.zw;
    }
#endif

    half4 shadowMask = 1.0;

    // Using SAMPLE_TEXTURE2D is faster than using LOAD_TEXTURE2D on iOS platforms (5% faster shader).
    // Possible reason: HLSLcc upcasts Load() operation to float, which doesn't happen for Sample()?
    float d        = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, my_point_clamp_sampler, screen_uv, 0).x; // raw depth value has UNITY_REVERSED_Z applied on most platforms.
    half4 gbuffer0 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, my_point_clamp_sampler, screen_uv, 0);
    half4 gbuffer1 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer1, my_point_clamp_sampler, screen_uv, 0);
    half4 gbuffer2 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, my_point_clamp_sampler, screen_uv, 0);

    half surfaceDataOcclusion = gbuffer1.a;
    uint materialFlags = UnpackMaterialFlags(gbuffer0.a);

    half3 color = 0;
    half alpha = 1.0;

#if defined(SUPPORTS_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
    UNITY_BRANCH if (_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
    {
        input.positionCS.xy = undistorted_screen_uv * _ScreenSize.xy;
    }
#endif

    #if defined(USING_STEREO_MATRICES)
    int eyeIndex = unity_StereoEyeIndex;
    #else
    int eyeIndex = 0;
    #endif
    float4 posWS = mul(_ScreenToWorld[eyeIndex], float4(input.positionCS.xy, d, 1.0));
    posWS.xyz *= rcp(posWS.w);

    Light unityLight = GetStencilLight(posWS.xyz, screen_uv, shadowMask, materialFlags);

#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(screen_uv);
#endif

    InputData inputData = InputDataFromGbufferAndWorldPosition(gbuffer2, posWS.xyz);

#if SHADER_API_MOBILE || SHADER_API_SWITCH
    // Specular highlights are still silenced by setting specular to 0.0 during gbuffer pass and GPU timing is still reduced.
    bool materialSpecularHighlightsOff = false;
#else
    bool materialSpecularHighlightsOff = (materialFlags & kMaterialFlagSpecularHighlightsOff);
#endif

    half3 test = 0;

    UNITY_LOOP
    for (int addIndex = 0; addIndex < GetRoxamiAdditionalLightsCount(); addIndex++)
    {
        Light light = GetAdditionalPerObjectLight(addIndex, inputData.positionWS);
        test += light.color * saturate(dot(light.direction, inputData.normalWS) * light.distanceAttenuation);
    }

    Light mainLight = GetMainLight();
    
    BRDFData brdfData = BRDFDataFromGbuffer(gbuffer0, gbuffer1, gbuffer2);
    color = LightingToonBased(brdfData, mainLight, inputData);

    color += test;

    return half4(color, alpha);
}

#endif