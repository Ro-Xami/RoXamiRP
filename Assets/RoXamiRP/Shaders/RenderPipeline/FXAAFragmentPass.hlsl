#ifndef ROXAMIRP_FXAA_FRAGMENT_PASS_INCLUDE
#define ROXAMIRP_FXAA_FRAGMENT_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/Shaders/RenderPipeline/SamplePostSource.hlsl"

float3 SampleCameraAttachment(float2 uv)
{
    return SAMPLE_TEXTURE2D_LOD(_PostSource0, sampler_linear_clamp, uv, 0).rgb;
}

float Luma(float2 uv)
{
    return GetLuma(SampleCameraAttachment(uv).rgb);
}

//==============================================================================================
//Quality

#define maxStep 15
#define guessStep 8

// This used to be the FXAA_QUALITY__EDGE_THRESHOLD_MIN define.
// It is here now to allow easier tuning.
// Trims the algorithm from processing darks.
//   0.0833 - upper limit (default, the start of visible unfiltered edges)
//   0.0625 - high quality (faster)
//   0.0312 - visible limit (slower)
// Special notes when using FXAA_GREEN_AS_LUMA,
//   Likely want to set this to zero.
//   As colors that are mostly not-green
//   will appear very dark in the green channel!
//   Tune by looking at mostly non-green content,
//   then start at zero and increase until aliasing is a problem.
//   _contrastThreshold 0.0312

// This used to be the FXAA_QUALITY__EDGE_THRESHOLD define.
// It is here now to allow easier tuning.
// The minimum amount of local contrast required to apply algorithm.
//   0.333 - too little (faster)
//   0.250 - low quality
//   0.166 - default
//   0.125 - high quality
//   0.063 - overkill (slower)
//  _relativeThreshold 0.125

#if defined(_AA_HIGH)
	#define _contrastThreshold_Quality 0.0312
	#define _relativeThreshold_Quality 0.125
#elif defined(_AA_MIDDLE)
	#define _contrastThreshold_Quality 0.0625
	#define _relativeThreshold_Quality 0.166
#elif defined(_AA_LOW)
	#define _contrastThreshold_Quality 0.0833
	#define _relativeThreshold_Quality 0.250
#endif

float4 FxaaQualityPixelShader(float2 uv, float contrastThreshold, float relativeThreshold)
{
    float3 mCol = SampleCameraAttachment(uv).rgb;
    float M = GetLuma(mCol);
    float N = Luma(uv + texelSize.xy * float2(0, 1));
    float S = Luma(uv + texelSize.xy * float2(0, -1));
    float W = Luma(uv + texelSize.xy * float2(-1, 0));
    float E = Luma(uv + texelSize.xy * float2(1, 0));

    float maxLumaNS = max(N, S);
    float maxLumaWE = max(W, E);
    float minLumaNS = min(N, S);
    float minLumaWE = min(W, E);

    float minLuma = min(M, min(minLumaNS, minLumaWE));
    float maxLuma = max(M, max(maxLumaNS, maxLumaWE));
    float contrastMinMax = maxLuma - minLuma;

    if (contrastMinMax < max(contrastThreshold, maxLuma * relativeThreshold))
    {
        return float4(mCol, 1);
    }
				
    float NW = Luma(uv + texelSize.xy * float2(-1, 1));
    float NE = Luma(uv + texelSize.xy * float2(1, 1));
    float SW = Luma(uv + texelSize.xy * float2(-1, -1));
    float SE = Luma(uv + texelSize.xy * float2(1, -1));

    float averageLuma = 2 * (N + S + W + E) + NW + NE + SW + SE;
    averageLuma /= 12;
    float contrastAverage = abs(averageLuma - M);
    float blendLuma = saturate(contrastAverage / contrastMinMax);
    blendLuma = blendLuma * blendLuma;

    float vertical = abs(N + S - 2 * M) * 2 + abs(NW + SW - 2 * W) + abs(NE + SE - 2 * E);
    float horizontal = abs(W + E - 2 * M) * 2 + abs(SW + SE - 2 * S) + abs(NW + NE - 2 * N);
    bool isHorizontal = vertical > horizontal;

    float contrastPositive = abs((isHorizontal ? N : E) - M);
    float contrastNegative = abs((isHorizontal ? S : W) - M);

    float searchStart, searchEndThreshold;
    float2 blendDir = isHorizontal ?
        float2(0, texelSize.y) :
        float2(texelSize.x, 0);

    if (contrastPositive > contrastNegative)
    {
        searchStart = (M + (isHorizontal ? N : E)) * 0.5;
        searchEndThreshold = contrastPositive * 0.25;
    }
    else
    {
        searchStart = (M + (isHorizontal ? S : W)) * 0.5;
        searchEndThreshold = contrastNegative * 0.25;
        blendDir *= -1;
    }

    float2 searchStartUV = uv + blendDir * 0.5;
    float2 searchStepDir = isHorizontal ?
        float2(texelSize.x, 0) :
        float2(0, texelSize.y);

    float searchStepPositive = 0, searchStepNegative = 0, searchLumaPositive = 0, searchLumaNegative = 0;

    int i;
    UNITY_UNROLL
    for (i = 1; i <= maxStep; i++)
    {
        float2 step = searchStepDir * i;
        searchLumaPositive = Luma(searchStartUV + step) - searchStart;
        if (abs(searchLumaPositive) > searchEndThreshold)
        {
            searchStepPositive = isHorizontal ? step.x : step.y;
            break;
        }
    }
    if (i == maxStep + 1)
    {
        searchStepPositive = (isHorizontal ? searchStepDir.x : searchStepDir.y) * (maxStep + 1);
    }

    UNITY_UNROLL
    for (i = 1; i <= maxStep; i++)
    {
        float2 step = searchStepDir * i;
        searchLumaNegative = Luma(searchStartUV - step) - searchStart;
        if (abs(searchLumaNegative) > searchEndThreshold)
        {
            searchStepNegative = isHorizontal ? step.x : step.y;
            break;
        }
    }
    if (i == maxStep + 1)
    {
        searchStepNegative = (isHorizontal ? searchStepDir.x : searchStepDir.y) * (maxStep + 1);
    }

    float blendEdge;
    if (searchStepPositive < searchStepNegative)
    {
        if (sign(searchLumaPositive) == sign(M - searchStart))
        {
            blendEdge = 0;
        }
        else
        {
            blendEdge = 0.5 - searchStepPositive / (searchStepPositive + searchStepNegative);
        }
    }
    else
    {
        if (sign(searchLumaNegative) == sign(M - searchStart))
        {
            blendEdge = 0;
        }
        else
        {
            blendEdge = 0.5 - searchStepNegative / (searchStepPositive + searchStepNegative);
        }
    }

    float blend = max(blendLuma, blendEdge);
    float4 result = float4(SampleCameraAttachment(uv + blendDir * blend), 1);

    return float4(result.rgb, 1);
}

float4 FXAA_Quality(Varyings IN) : SV_Target
{
    float4 col;
#if defined(_contrastThreshold_Quality) && defined(_relativeThreshold_Quality)
    col = FxaaQualityPixelShader(IN.uv, _contrastThreshold_Quality, _relativeThreshold_Quality);
#else
    col = float4(SampleCameraAttachment(IN.uv).rgb, 1);
#endif

    return col;
}

//============================================================================================
//Console

// Only used on FXAA Console.
// This used to be the FXAA_CONSOLE__EDGE_THRESHOLD_MIN define.
// It is here now to allow easier tuning.
// Trims the algorithm from processing darks.
// The console setting has a different mapping than the quality setting.
// This only applies when FXAA_EARLY_EXIT is 1.
// This does not apply to PS3, 
// PS3 was simplified to avoid more shader instructions.
//   0.06 - faster but more aliasing in darks
//   0.05 - default
//   0.04 - slower and less aliasing in darks
// Special notes when using FXAA_GREEN_AS_LUMA,
//   Likely want to set this to zero.
//   As colors that are mostly not-green
//   will appear very dark in the green channel!
//   Tune by looking at mostly non-green content,
//   then start at zero and increase until aliasing is a problem.
//  #define _contrastThreshold 0.05

// Only used on FXAA Console.
// This used to be the FXAA_CONSOLE__EDGE_THRESHOLD define.
// It is here now to allow easier tuning.
// This does not effect PS3, as this needs to be compiled in.
//   Use FXAA_CONSOLE__PS3_EDGE_THRESHOLD for PS3.
//   Due to the PS3 being ALU bound,
//   there are only two safe values here: 1/4 and 1/8.
//   These options use the shaders ability to a free *|/ by 2|4|8.
// The console setting has a different mapping than the quality setting.
// Other platforms can use other values.
//   0.125 leaves less aliasing, but is softer (default!!!)
//   0.25 leaves more aliasing, and is sharper
//  #define _relativeThreshold 0.125

#if defined(_AA_HIGH)
#define _contrastThreshold_Console 0.04
#define _relativeThreshold_Console 0.125
#elif defined(_AA_MIDDLE)
#define _contrastThreshold_Console 0.05
#define _relativeThreshold_Console 0.25
#elif defined(_AA_LOW)
#define _contrastThreshold_Console 0.06
#define _relativeThreshold_Console 0.25
#endif

#define scale 0.5
#define sharpness 8

float4 FxaaConsolePixelShader(float2 uv, float contrastThreshold, float relativeThreshold)
{
    float3 mCol = SampleCameraAttachment(uv).rgb;
    float M = GetLuma(mCol);
    float NW = Luma(uv + float2(-0.5, 0.5) * texelSize);
    float NE = Luma(uv + float2(0.5, 0.5) * texelSize);
    float SW = Luma(uv + float2(-0.5, -0.5) * texelSize);
    float SE = Luma(uv + float2(0.5, -0.5) * texelSize);
    
    float maxLuma = max(max(NW, NE), max(SW, SE));
    float minLuma = min(min(NW, NE), min(NW, NE));
    float contrastMinMax = max(maxLuma, M) -  min(minLuma, M);
    
    if(contrastMinMax < max(contrastThreshold, maxLuma * relativeThreshold))
    {
        return float4(mCol, 1);
    }
    
    NE += 1.0f / 384.0f;//?
    float2 dir;
    dir.x = abs(NW + NE) - abs(SW + SE);
    dir.x *= -1;
    dir.y = abs(NE + SE) - abs(SW + NW);
    dir = normalize(dir);
    
    float2 dir1 = dir * texelSize * scale;
    
    float4 negative1 = float4(SampleCameraAttachment(uv - dir1), 1);
    float4 positive1 = float4(SampleCameraAttachment(uv + dir1), 1);
    float4 result1 = (negative1 + positive1) * 0.5;
    
    
    float factor = min(abs(dir1.x), abs(dir1.y)) * sharpness;
    float2 dir2 = clamp(dir1.xy / factor, -2.0, 2.0) * 2 * texelSize;
    float4 negative2 = float4(SampleCameraAttachment(uv - dir2), 1);
    float4 positive2 = float4(SampleCameraAttachment(uv + dir2), 1);
    float4 result2 = result1 * 0.5f + (negative2 + positive2) * 0.25f;
    
    float newLum = GetLuma(result2.rgb);
    
    float4 result =
        newLum >= minLuma && newLum <= maxLuma?
        result2 : result1;
    
    return float4(result.rgb, 1);
}

float4 FXAA_Console(Varyings IN) : SV_Target
{
    float4 col;
#if defined(_contrastThreshold_Console) && defined(_relativeThreshold_Console)
    col = FxaaConsolePixelShader(IN.uv, _contrastThreshold_Console, _relativeThreshold_Console);
#else
    col = float4(SampleCameraAttachment(IN.uv).rgb, 1);
#endif
    return col;
}
#endif