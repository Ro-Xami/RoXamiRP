#ifndef ROXAMIRP_SCREENSPACE_LIGHTING_INCLUDE
#define ROXAMIRP_SCREENSPACE_LIGHTING_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_ScreenSpaceShadowsTexture);
SAMPLER(sampler_ScreenSpaceShadowsTexture);

float SampleScreenSpaceShadows(float2 screenSpaceUV)
{
    return SAMPLE_TEXTURE2D(_ScreenSpaceShadowsTexture, sampler_ScreenSpaceShadowsTexture, screenSpaceUV).r;
}

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

#endif