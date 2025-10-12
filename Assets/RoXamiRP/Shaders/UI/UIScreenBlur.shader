Shader "RoXami RP/UI/UIScreenBlur"
{
	Properties
	{
		//_BaseColor ("Base Color" , color) = (1,1,1,1)
		[HiedInInspector] _MainTex ("Base Map" , 2D) = "white" {}
	}
	
	SubShader
	{
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "ToonUnlit"
			Tags
			{
				"LightMode" = "ToonUnlit"
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			HLSLPROGRAM
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			//#include "Assets/RoXamiRP/ShaderLibrary/CameraAttachment.hlsl"

			TEXTURE2D(_ScreenShotBlurTexture);
			SAMPLER(sampler_ScreenShotBlurTexture);

			CBUFFER_START(UnityPerMaterial)
				//float4 _BaseColor;
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
				//float4 srcPos : TEXCOORD1;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Varyings UnlitPassVertex(Attributes IN)
			{
				Varyings OUT = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

				half3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
				OUT.positionCS = TransformWorldToHClip(positionWS);
				OUT.color = IN.color;// * _BaseColor;
				OUT.uv = IN.uv;
				//OUT.srcPos = ComputeScreenPos(OUT.positionCS);

				return OUT;
			}

			float4 UnlitPassFragment (Varyings IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				float4 blurUI = SAMPLE_TEXTURE2D(_ScreenShotBlurTexture, sampler_ScreenShotBlurTexture, IN.uv);
				blurUI.a = 1;
				blurUI *= IN.color;

				return blurUI;
			}

			ENDHLSL
		}
	}
}