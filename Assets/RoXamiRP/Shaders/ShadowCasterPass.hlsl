#ifndef ROXAMIRP_TOONLITPASS_INCLUDE
#define ROXAMIRP_TOONLITPASS_INCLUDE

#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/ToonLitSurface.hlsl"
#include "../ShaderLibrary/ToonLighting.hlsl"

#pragma multi_compile_instancing
#pragma shader_feature_local _ALPHACLIP_ON

//UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
//	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
//	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
//	UNITY_DEFINE_INSTANCED_PROP(float, _cutout)
//UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

//#ifdef INSTANCING_ON
//    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
//        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
//    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

//#define _BaseColor              UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor)
//#endif

struct Attributes {
	float4 positionOS : POSITION;
	float2 uv : TEXCOORD0;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};
 
struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ShadowCasterPassVertex(Attributes IN)
{
	Varyings OUT = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

    OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
	OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

	#if UNITY_REVERSED_Z
		OUT.positionCS.z =
			min(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
	#else
		OUT.positionCS.z =
			max(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
	#endif

	return OUT;
}

real ShadowCasterPassFragment(Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);
	
	#ifdef _ALPHACLIP_ON
	half albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).a;
	clip(albedo.a - _cutout);
	#endif
	
    return 0;
}

#endif