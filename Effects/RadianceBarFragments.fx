float progress;
float4 color;
texture sampleTexture1;
sampler2D samplerTex1 = sampler_state
{
    texture = <sampleTexture1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};
texture sampleTexture2;
sampler2D samplerTex2 = sampler_state
{
    texture = <sampleTexture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};
texture sampleTexture3;
sampler2D samplerTex3 = sampler_state
{
    texture = <sampleTexture3>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};
float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput1 = tex2D(samplerTex1, uv);
    float4 fragmentOutput1 = tex2D(samplerTex2, uv);
    float4 fragmentOutput2 = tex2D(samplerTex3, uv);
    
    if(progress > 0.0f)
    {
        return colorOutput1 * lerp(float4(1, 1, 1, 1), fragmentOutput1, (float4)progress) * color;
    }
    return colorOutput1 * lerp(float4(1, 1, 1, 1), fragmentOutput2, (float4)progress * -1) * color;
    
}

technique Technique1
{
    pass RadianceBarFragmentsPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};