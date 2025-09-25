#ifndef ROXAMIRP_TOONLITPASS_INCLUDE
#define ROXAMIRP_TOONLITPASS_INCLUDE

#include "../ShaderLibrary/Common.hlsl"

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
	clip(albedo - _cutout);
	#endif
	
    return 0;
}

#endif