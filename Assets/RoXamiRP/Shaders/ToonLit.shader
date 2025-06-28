Shader "RoXami RP/ToonLit"
{
	Properties
	{
		_BaseColor ("Base Color" , color) = (1,1,1,1)
		_BaseMap ("Base Map" , 2D) = "white" {}
		_roughness ("Roughness" , Range(0 , 1)) = 0.5
		_metallic ("Metallic" , Range(0 , 1)) = 0
		_ao ("AO" , Range(0 , 1)) = 1
		[HDR]_emissive ("Emissive" , Color) = (0,0,0,0)
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		[Toggle] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5

		_TestCube ("TestCube" , Cube) = "white" {}
	}
	
	SubShader
	{
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "ToonLit"
			Tags {"LightMode" = "ToonLit"}
			ZWrite [_ZWrite]
			Blend [_SrcBlend] [_DstBlend]
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ToonLitPassVertex
			#pragma fragment ToonLitPassFragment

			#include_with_pragmas "Assets/RoXamiRP/Shaders/ToonLitPass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "ToonGBuffer"
			Tags{"LightMode" = "ToonGBuffer"}
			
			Stencil
			{
				Ref 0
				Pass Replace
			}
			
			HLSLPROGRAM
			#pragma vertex ToonGBufferPassVertex
			#pragma fragment ToonGBufferPassFragment

			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

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

				return OUT;
			}

			void ToonGBufferPassFragment (Varyings IN,
				out float4 GT0 : SV_Target0,
                out float4 GT1 : SV_Target1,
                out float4 GT2 : SV_Target2,
                out float4 GT3 : SV_Target3)
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				GT0 = 0;
				GT1 = 0;
				GT2 = 0;
				GT3 = 0;
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