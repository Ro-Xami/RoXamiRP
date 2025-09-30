#ifndef ROXAMIRP_SMAA_FRAGMENT_PASS_INCLUDE
#define ROXAMIRP_SMAA_FRAGMENT_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/Shaders/RenderPipeline/SampleTempRtSource.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float3 SampleCameraAttachment(float2 uv)
{
    return SAMPLE_TEXTURE2D_LOD(_TempRtSource0, sampler_linear_clamp, uv, 0).rgb;
}

float4 SampleCameraAttachmentRGBA(float2 uv)
{
    return SAMPLE_TEXTURE2D_LOD(_TempRtSource0, sampler_linear_clamp, uv, 0);
}

float Luma(float2 uv)
{
    return Luminance(SampleCameraAttachment(uv).rgb);
}

#if defined(_AA_HIGH)
    #define _edgeThreshold 0.03f
    #define _maxStep 10
#elif defined(_AA_MIDDLE)
    #define _edgeThreshold 0.04f
    #define _maxStep 8
#elif defined(_AA_LOW)
    #define _edgeThreshold 0.05f
    #define _maxStep 6
#endif


float4 SMAA_Edge(Varyings IN) : SV_Target
{
    float2 uv = IN.uv.xy;
    float M = Luma(uv);
    float N =   abs(Luma(uv + float2(0  , 1 ) * texelSize) - M);
    float S =   abs(Luma(uv + float2(0  , -1) * texelSize) - M);
    float W =   abs(Luma(uv + float2(-1 , 0 ) * texelSize) - M);
    float E =   abs(Luma(uv + float2(1  , 0 ) * texelSize) - M);
    float wW =  abs(Luma(uv + float2(-2 , 1 ) * texelSize) - M);
    float sS =  abs(Luma(uv + float2(0  , -2) * texelSize) - M);

    float roundMax = max(max(N, S), max(E, W));
    
    bool isW = W > _edgeThreshold;
    isW = isW && W > (max(roundMax, wW) * 0.5f);

    bool isS = S > _edgeThreshold;
    isS = isS && S > (max(roundMax, sS) * 0.5f);

    float2 edge = 0;
    edge.x = isW ? 1 : 0;
    edge.y = isS ? 1 : 0;
    
    return float4(edge, 0, 1);
}

float SearchLeftX(float2 uv)
{
    uv = uv - float2(1.5, 0) * texelSize;
    float2 move = float2(2, 0) * texelSize;
    
    float edge = 0;
    int i = 0;
    
    UNITY_UNROLL
    for (i = 0; i < _maxStep; i++)
    {
        edge = SampleCameraAttachment(uv).g;
        
        [flatten]
        if (edge < 0.9)
        {
            break;
        }
        uv -= move;
    }
    return min(2.0 * (i + edge), 2.0 * _maxStep);
}

float SearchRightX(float2 uv)
{
    uv = uv + float2(1.5, 0) * texelSize;
    float2 move = float2(2, 0) * texelSize;
    float edge = 0;
    int i = 0;

    UNITY_UNROLL
    for (i = 0; i < _maxStep; i++)
    {
        edge = SampleCameraAttachment(uv).g;
        
        [flatten]
        if (edge < 0.9)
        {
            break;
        }
        uv += move;
    }
    return min(2.0 * (i + edge), 2.0 * _maxStep);
}

float SearchUpY(float2 uv)
{
    uv = uv - float2(0, 1.5) * texelSize;
    float2 move = float2(0, 2) * texelSize;
    float edge = 0;
    int i = 0;

    UNITY_UNROLL
    for (i = 0; i < _maxStep; i++)
    {
        edge = SampleCameraAttachment(uv).r;
        
        [flatten]
        if (edge < 0.9)
        {
            break;
        }
        uv -= move;
    }
    return min(2.0 * (i + edge), 2.0 * _maxStep);
}

float SearchBottomY(float2 uv)
{
    uv = uv + float2(0, 1.5) * texelSize;
    float2 move = float2(0, 2) * texelSize;
    float edge = 0;
    int i = 0;

    UNITY_UNROLL
    for (i = 0; i < _maxStep; i++)
    {
        edge = SampleCameraAttachment(uv).r;
        
        [flatten]
        if (edge < 0.9)
        {
            break;
        }
        uv += move;
    }
    return min(2.0 * (i + edge), 2.0 * _maxStep);
}

//use bilinear mode, sample point move bottom 0.25 pixels
//ff(down false; up false)  : 0
//ft(down false; up true)   : 0.25
//tf(down true; up false)   : 0.75
//tt(down true; up true)    : 1
bool2 ModeOfSingle(float value)
{
    bool2 mode = false;
    if (value > 0.875)
        mode.xy = bool2(true, true);
    else if(value > 0.5)
        mode.y = true;
    else if(value > 0.125)
        mode.x = true;
    return mode;
}

//  |____
float L_N_Shape(float d, float m)
{
    float l = d * 0.5;
    float s = 0;
    [flatten]
    if ( l > (m + 0.5))
    {
        s = (l - m) * 0.5 / l;
    }
    else if (l > (m - 0.5))
    {
        float a = l - m + 0.5;
        s = a * a * 0.25 * rcp(l);
    }
    return s;
}

//  |____|
float L_L_S_Shape(float d1, float d2)
{
    float d = d1 + d2;
    float s1 = L_N_Shape(d, d1);
    float s2 = L_N_Shape(d, d2);
    return s1 + s2;
}

//  |____    |___|    
//       |       |
float L_L_D_Shape(float d1, float d2)
{
    float d = d1 + d2;
    float s1 = L_N_Shape(d, d1);
    float s2 = -L_N_Shape(d, d2);
    return s1 + s2;
}

float GetArea(float leftValue, float rightValue, bool2 leftMode, bool2 rightMode)
{
    float result = 0;
    [branch]
    if(!leftMode.x && !leftMode.y)
    {
        [branch]
        if(rightMode.x && !rightMode.y)
        {
            result = L_N_Shape(rightValue + leftValue + 1, rightValue + 0.5);
        }
        else if (!rightMode.x && rightMode.y)
        {
            result = -L_N_Shape(rightValue + leftValue + 1, rightValue + 0.5);
        }
    }
    else if (leftMode.x && !leftMode.y)
    {
        [branch]
        if(rightMode.y)
        {
            result = L_L_D_Shape(leftValue + 0.5, rightValue + 0.5);
        }
        else if (!rightMode.x)
        {
            result = L_N_Shape(rightValue + leftValue + 1, leftValue + 0.5);
        }
        else
        {
            result = L_L_S_Shape(leftValue + 0.5, rightValue + 0.5);
        }
    }
    else if (!leftMode.x && leftMode.y)
    {
        [branch]
        if (rightMode.x)
        {
            result = -L_L_D_Shape(leftValue + 0.5, rightValue + 0.5);
        }
        else if (!rightMode.y)
        {
            result = -L_N_Shape(leftValue + rightValue + 1, leftValue + 0.5);
        }
        else
        {
            result = -L_L_S_Shape(leftValue + 0.5, rightValue + 0.5);
        }
    }
    else
    {
        [branch]
        if(rightMode.x && !rightMode.y)
        {
            result = -L_L_D_Shape(leftValue + 0.5, rightValue + 0.5);
        }
        else if (!rightMode.x && rightMode.y)
        {
            result = L_L_D_Shape(leftValue + 0.5, rightValue + 0.5);
        }
    }
    return result;
}

float4 SMAA_Factor(Varyings IN) : SV_Target
{
    float2 uv = IN.uv.xy;
    float2 edge = SampleCameraAttachment(uv).rg;
    float4 factor = 0;

    if (edge.g > 0.1f)
    {
        float leftStep = SearchLeftX(uv);
        float rightStep = SearchRightX(uv);

        float leftValue = SampleCameraAttachment(uv + float2(-leftStep, -0.25) * texelSize).r;
        float rightValue = SampleCameraAttachment(uv + float2(rightStep + 1, -0.25) * texelSize).r;
        
        bool2 leftMode = ModeOfSingle(leftValue);
        bool2 rightMode = ModeOfSingle(rightValue);

        float value = GetArea(leftStep, rightStep, leftMode, rightMode);
        factor.xy = float2(-value, value);
    }
    
    if (edge.r > 0.1f)
    {
        float upStep = SearchUpY(uv);
        float bottomStep = SearchBottomY(uv);

        float upValue = SampleCameraAttachment(uv + float2(-0.25, -upStep) * texelSize).g;
        float bottomValue = SampleCameraAttachment(uv + float2( -0.25, bottomStep + 1) * texelSize).g;
        
        bool2 upMode = ModeOfSingle(upValue);
        bool2 bottomMode = ModeOfSingle(bottomValue);

        float value = GetArea(upStep, bottomStep, upMode, bottomMode);
        factor.zw = float2(-value, value);
    }

    return factor;
}

TEXTURE2D(_SmaaFactorRT);
SAMPLER(sampler_SmaaFactorRT);

float4 SMAA_Blend(Varyings IN) : SV_Target
{
    float2 uv = IN.uv;
    //return float4(SampleCameraAttachment(uv), 1);
    int2 pixelCoord = uv * _TempRtSource0_TexelSize.zw;
    float4 TL = _SmaaFactorRT.Load(int3(pixelCoord, 0));
    float R = _SmaaFactorRT.Load(int3(pixelCoord + int2(1, 0), 0)).a;
    float B = _SmaaFactorRT.Load(int3(pixelCoord + int2(0, 1), 0)).g;

    float4 a = float4(TL.r, B, TL.b, R);
    float4 w = a * a * a;
    float sum = dot(w, 1.0);

    [branch]
    if (sum > 0) {
        float4 o = a * texelSize.yyxx;
        float4 color = 0;

        color = mad(SampleCameraAttachmentRGBA(uv + float2(0.0, -o.r)), w.r, color);
        color = mad(SampleCameraAttachmentRGBA(uv + float2( 0.0, o.g)), w.g, color);
        color = mad(SampleCameraAttachmentRGBA(uv + float2(-o.b, 0.0)), w.b, color);
        color = mad(SampleCameraAttachmentRGBA(uv + float2( o.a, 0.0)), w.a, color);
        return color/sum;
    } else
    {
        return float4(SampleCameraAttachment(uv), 1);
    }
}

#endif