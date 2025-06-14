Shader "RoXami RP/Unlit"
{
	Properties
	{
		_BaseColor ("Base Color" , color) = (1,1,1,1)
		_BaseMap ("Base Map" , 2D) = "white" {}
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		[Toggle] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5
	}
	
	SubShader
	{
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "ToonUnlit"
			Tags{"LightMode" = "ToonUnlit"}
			ZWrite [_ZWrite]
			Blend [_SrcBlend] [_DstBlend]
			HLSLPROGRAM
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/CameraAttachment.hlsl"

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float4 _BaseMap_ST;
				float _cutout;
			CBUFFER_END

			#ifdef INSTANCING_ON
			    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
			        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
			    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			#define _BaseColor              UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor)
			#endif

			struct Attributes {
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			 
			struct Varyings {
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Varyings UnlitPassVertex(Attributes IN)
			{
				Varyings OUT = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

				half3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
				OUT.positionCS = TransformWorldToHClip(positionWS);
				OUT.color = IN.color * _BaseColor;
				OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

				return OUT;
			}

			float4 UnlitPassFragment (Varyings IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor * IN.color;

				#ifdef _ALPHACLIP_ON
				clip(albedo.a - _cutout);
				#endif

				float depth = SampleCameraDepth(IN.uv);
				float3 color = SampleCameraOpaqueColor(IN.uv);

				return float4(color,1);
				return float4(depth.xxx, 1);
				return albedo;
			}

			ENDHLSL
		}
	}
}