float4 color1;
float4 color2;
float time;
texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput = tex2D(samplerTex, uv);
    if (colorOutput.w != 0)
    {   
        float4 newColor = lerp(colorOutput, color2, time);
        newColor *= color1;
        return newColor;
    }
    return colorOutput;
}

technique Technique1
{
    pass DisintegrationPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};