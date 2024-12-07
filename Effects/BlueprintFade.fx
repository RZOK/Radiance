float progress;
float fadeThreshold;
bool pixelate;
float2 resolution;

texture sampleTexture;
sampler2D samplerTex = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

float GetLerpValue(float from, float to, float t)
{
    return (t - from) / (to - from);
}
float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput = tex2D(samplerTex, uv);
    if(pixelate)
    {
        uv.x -= uv.x % (1 / resolution.x);
        uv.y -= uv.y % (1 / resolution.y);
    }
    float combinedUV = uv.x + uv.y;
    if(combinedUV > 1)
    {
        combinedUV = 2 - (uv.x + uv.y);
    }
    
    if (combinedUV < progress)
    {
        float mod = 1;
        if (combinedUV > progress - fadeThreshold)
        {
            mod = lerp(1, 0, GetLerpValue(progress - fadeThreshold, progress, combinedUV));
        }
        return colorOutput * mod;
    }
    
    return float4(0, 0, 0, 0);
}

technique Technique1
{
    pass BlueprintFadePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};