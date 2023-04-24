float4 color1;
float4 color2;
float4 color3;
float offset;
texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput = float4(0, 0, 0, 0);
    float lerpAmount = ((uv.x + uv.y) / 2 + offset) % 1;
    if (lerpAmount < 0.33f)
    {
        colorOutput = lerp(color1, color2, lerpAmount / 0.33f);
    }
    else if (lerpAmount < 0.66f)
    {
        colorOutput = lerp(color2, color3, (lerpAmount - 0.33f) / 0.33f);
    }
    else
    {
        colorOutput = lerp(color3, color1, (lerpAmount - 0.66f) / 0.33f);
    }
    
    float4 color = tex2D(samplerTex, uv);
    if (color.w != 0)
    {   
        return colorOutput;
    }
    return color;
}

technique Technique1
{
    pass AuroraPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};