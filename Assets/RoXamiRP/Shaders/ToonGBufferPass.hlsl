#ifndef ROXAMIRP_TOON_GBUFFER_PASS_INCLUDE
#define ROXAMIRP_TOON_GBUFFER_PASS_INCLUDE

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
        _emissive ,
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

#endif