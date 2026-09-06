texture sampleTexture;
sampler2D texSampler = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};
float4 color;
float alpha;

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 textureColor = tex2D(texSampler, uv);
    if (textureColor.w != 0)
    {
        return color;
    }
    return textureColor;
}

technique Technique1
{
    pass SolidColorPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};