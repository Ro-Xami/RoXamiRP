Shader "RoXamiRP/Hide/DeferredToonLit"
{
	Properties
	{
		[NoScaleOffset] _ToonLitLut ("Lut", 2D) = "white" {}
	}
	
	SubShader
	{
		Pass
		{
			Name "DeferredToonLitDiffuse"
			
			Stencil
			{
				Ref 100
				Comp Equal
			}
			
			Cull Off
			ZWrite Off
			ZTest Always
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment DeferredToonLitPassFragment
			#pragma multi_compile _ SCREENSPACE_SHADOWS
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/DeferredLit/DeferredInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/Common/ToonLighting.hlsl"

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

			float4 DeferredToonLitPassFragment (Varyings IN) : SV_TARGET
			{
				Surface surfaceData = GetSurfaceData(IN);
				float depth;
				Input inputData = GetInputData(IN, surfaceData, depth);

				float4 color = CalculateDeferredToonLitDiffuseEmission(inputData , surfaceData);

			    return float4(color.rgb, 1);
			}

			ENDHLSL
		}

		Pass
		{
			Name "DeferredToonLitGI"
			
			Stencil
			{
				Ref 100
				Comp Equal
			}
			
			Cull Off
			ZWrite Off
			ZTest Always
			Blend One One
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment DeferredToonLitPassFragment
			#pragma multi_compile _ SCREENSPACE_REFLECTION
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/DeferredLit/DeferredInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/Common/ToonLighting.hlsl"

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

			float4 DeferredToonLitPassFragment (Varyings IN) : SV_TARGET
			{
				Surface surfaceData = GetSurfaceData(IN);
				float depth;
				Input inputData = GetInputData(IN, surfaceData, depth);

				float4 color = CalculateDeferredToonLitGI(inputData , surfaceData);

			    return float4(color.rgb, 1);
			}

			ENDHLSL
		}
	}
}