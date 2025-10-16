#ifndef ROXAMIRP_RENDER_DEBUG_INCLUDE
#define ROXAMIRP_RENDER_DEBUG_INCLUDE

// void RoXamiRpRenderDebug(inout float3 finalColor)
// {
// }
//
// void RoXamiRpRenderDebug(Surface surface, Input input, inout float3 finalColor)
// {
// }

#pragma shader_feature _ _Debug_None _Debug_Albedo _Debug_Normal _Debug_Metallic _Debug_Roughness _Debug_Ao _Debug_Emission _Debug_GiDiffuse _Debug_GiSpecular _Debug_Shadow

#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Input.hlsl"

void RoXamiRpRenderDebug(inout float3 finalColor)
{
    #ifdef _Debug_None
    #else
    finalColor = 0;
    #endif
}

void RoXamiRpRenderDebug(Surface surface, Input input, inout float3 finalColor)
{
    #ifdef _Debug_Albedo
    finalColor = surface.albedo;
    #endif

    #ifdef _Debug_Normal
    finalColor = surface.normal;
    #endif

    #ifdef _Debug_Metallic
    finalColor = surface.metallic.xxx;
    #endif

    #ifdef _Debug_Roughness
    finalColor = surface.roughness.xxx;
    #endif

    #ifdef _Debug_Ao
    finalColor = surface.ao.xxx;
    #endif

    #ifdef _Debug_Emission
    finalColor = surface.emissive;
    #endif
}

#endif 