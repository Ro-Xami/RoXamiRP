#ifndef _TOON_DEFERRED_FRAGMENT
#define _TOON_DEFERRED_FRAGMENT

#include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/Utils/StencilDeferred.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/Toon/Core/ToonLighting.hlsl"

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

#if defined(_SCREEN_SPACE_OCCLUSION) && !defined(_SURFACE_TYPE_TRANSPARENT)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(screen_uv);
#endif

    InputData inputData = InputDataFromGbufferAndWorldPosition(gbuffer2, posWS.xyz);

    //全部渲染高光
    //bool materialSpecularHighlightsOff = (materialFlags & kMaterialFlagSpecularHighlightsOff);

    BRDFData brdfData = BRDFDataFromGbuffer(gbuffer0, gbuffer1, gbuffer2);

    Light mainLight = GetToonDeferredMainLight(inputData.positionWS, screen_uv);

    half4 color = 0;
    color.rgb += LightingToonBased(brdfData, mainLight, inputData);

    UNITY_LOOP
    for (int addIndex = 0; addIndex < GetRoxamiAdditionalLightsCount(); addIndex++)
    {
        Light additionalLight = GetAdditionalPerObjectLight(addIndex, inputData.positionWS);
        color.rgb += LightingToonBased(brdfData, additionalLight, inputData);
    }

    color.a = 1;

    return color;
}

#endif