Shader "RoXamiRP/Deferred/DeferredActorLit"
{
	Properties
	{
		[Header(Lut)]
		_ActorLutMap ("Actor Common Map", 2D) = "white" {}
		_ActorSkinMap ("Actor Face Map", 2D) = "white" {}
		
		[Space(10)]
		[Header(OutLine)]
		_ActorRimColor ("Rim Color", Color) = (1, 1, 1, 1)
		_ActorRimOffest ("Rim Offest", Float) = 1
		_ActorRimThreshold ("Rim Threshold", Range(0, 1)) = 0.5
	}
	
	SubShader
	{
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "DeferredActorLit"
			
			Stencil
			{
				Ref 101
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

			    return color;
			}
			ENDHLSL
		}

		Pass
		{
			Name "DeferredActorSkin"
			
			Stencil
			{
				Ref 102
				Comp Equal
			}

			Cull Off
			ZWrite Off
			ZTest Always
		
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment DeferredActorSkinPass
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
			

			float4 DeferredActorSkinPass (Varyings IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				Surface surfaceData = GetSurfaceData(IN);
				float depth;
				Input inputData = GetInputData(IN, surfaceData, depth);

				float4 color = CalculateActorSkin(inputData , surfaceData, depth);

			    return color;
			}
			ENDHLSL
		}
	}
}