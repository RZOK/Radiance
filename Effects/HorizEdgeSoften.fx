float fadeThreshold;
bool pixelate;
float4 color;
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
    float4 colorOutput = tex2D(samplerTex, uv) * color;
    float distFromCenter = abs(uv.x - 0.5) * 2;
    if(distFromCenter > fadeThreshold)
    {
        return colorOutput * GetLerpValue(1, fadeThreshold, distFromCenter);
    }
    return colorOutput;
}

technique Technique1
{
    pass HorizEdgeSoftenPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};