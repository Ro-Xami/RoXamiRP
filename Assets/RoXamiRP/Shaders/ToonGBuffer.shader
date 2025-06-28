Shader "RoXami RP/ToonGBuffer"
{
	Properties
	{
		_BaseColor ("Base Color" , color) = (1,1,1,1)
		_BaseMap ("Base Map" , 2D) = "white" {}
		
		_NormalMap ("Normal Map" , 2D) = "Bump" {}
		_normalStrength ("Normal Strength"  , Float) = 1
		
		_MRAMap ("MRA Map" , 2D) = "blue" {}
		_metallic ("Metallic" , Range(0 , 1)) = 1
		_roughness ("Roughness" , Range(0 , 1)) = 1
		_ao ("AO" , Range(0 , 1)) = 1
		
		_EmissionMap ("Emission Map" , 2D) = "black" {}
		_emissionColor ("Emission Color" , Color) = (1 , 1 , 1 , 1)
		
		[Toggle] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5
	}
	
	SubShader
	{
		HLSLINCLUDE
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
		
		ENDHLSL

		Pass
		{
			Name "ToonGBuffer"
			Tags{"LightMode" = "ToonGBuffer"}
			
			Stencil
			{
				Ref 100
				Pass Replace
			}
			
			HLSLPROGRAM
			#pragma vertex ToonGBufferPassVertex
			#pragma fragment ToonGBufferPassFragment

			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);
			TEXTURE2D(_MRAMap);
			SAMPLER(sampler_MRAMap);
			TEXTURE2D(_NormalMap);
			SAMPLER(sampler_NormalMap);
			TEXTURE2D(_EmissionMap);
			SAMPLER(sampler_EmissionMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float4 _BaseMap_ST;
				float _metallic;
				float _roughness;
				float _ao;
				float _normalStrength;
				float3 _emissionColor;
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
				float3 normalOS : NORMAL;
				float3 tangentOS : TANGENT;
				float4 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			 
			struct Varyings {
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				float3 tangentWS : TEXCOORD2;
				float4 bitangentWS : TEXCOORD3;
				float4 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Varyings ToonGBufferPassVertex(Attributes IN)
			{
				Varyings OUT = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

				OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
				OUT.color = IN.color * _BaseColor;
				OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

				return OUT;
			}

			void ToonGBufferPassFragment (Varyings IN,
				out float4 GT0 : SV_Target0,
                out float4 GT1 : SV_Target1,
                out float4 GT2 : SV_Target2,
                out float4 GT3 : SV_Target3)
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				float4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * IN.color;
				
				float4 mra =
					SAMPLE_TEXTURE2D(_MRAMap, sampler_MRAMap, IN.uv) *
					float4(_metallic , _roughness , _ao , 1);

				float4 emission = float4(
					SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).rgb *
					_emissionColor ,
					1);

				float4 normal = float4(IN.normalWS , 1);

				#ifdef _ALPHACLIP_ON
				clip(albedo.a - _cutout);
				#endif

				GT0 = albedo;
				GT1 = normal;
				GT2 = mra;
				GT3 = emission;
			}

			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags {"LightMode" = "ShadowCaster"}
			ColorMask 0
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment

			#include_with_pragmas "Assets/RoXamiRP/Shaders/ShadowCasterPass.hlsl"

			ENDHLSL
		}
	}
}