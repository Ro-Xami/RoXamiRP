Shader "RoXami RP/Deferred/DeferredActorLit"
{
	Properties
	{
		_LutMap ("Lut Map", 2D) = "white" {}
		[Header(Toon)]
		_rimColor ("Rim Color", Color) = (1, 1, 1, 1)
		_rimOffest ("Rim Offest", Float) = 1
		_rimThreshold ("Rim Threshold", Range(0, 1)) = 0.5
		
		//[HideInInspector] _DeferredActorLitStencil ("", int) = 0
	}
	
	SubShader
	{
		HLSLINCLUDE
		//#include "Assets/RoXamiRP/Shaders/Common/CameraDepthAttachment.hlsl"
		//int	_DeferredActorLitStencil;
		ENDHLSL
		Pass
		{
			Name "DeferredActorLit"
			
			Stencil
			{
				Ref 0
				Comp Equal
			}

			Cull Off
			ZWrite Off
			ZTest Always
		
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment DeferredActorLitPass
			#pragma multi_compile _ SCREENSPACE_SHADOWS
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/DeferredLit/DeferredInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/Common/ActorLighting.hlsl"

			Surface GetSurfaceData(Varyings IN)
			{
				Surface OUT = (Surface)0;
				float4 base = SampleAlbedo(IN.uv);
				float4 nomral = SampleNormal(IN.uv);
				float4 MRA = SampleMRA(IN.uv);
				float4 emission = SamplerEmission(IN.uv);
				OUT.albedo = base.rgb;
				OUT.normal = nomral.xyz;
				OUT.roughness = MRA.g;
				OUT.metallic = MRA.r;
				OUT.ao = MRA.b;
				OUT.emissive = emission.rgb;
				OUT.alpha = base.a;

				return OUT;
			}
			

			float4 DeferredActorLitPass (Varyings IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				Surface surfaceData = GetSurfaceData(IN);
				float depth;
				Input inputData = GetInputData(IN, surfaceData, depth);

				float4 color = CalculateActorLighting(inputData , surfaceData, depth);

				#ifdef _ALPHACLIP_ON
					clip(surfaceData.alpha - _cutout);
				#endif

			    return color;
			}
			

			ENDHLSL
		}
	}
}