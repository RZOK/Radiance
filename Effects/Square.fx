float4 color;
float halfWidth;

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput = float4(0, 0, 0, 0);
    float size = 1.5 / halfWidth;
    if ((uv.x < size || uv.x > 1 - size) || (uv.y < size || uv.y > 1 - size))
    {
        colorOutput = color;
    }
    return colorOutput;

}

technique Technique1
{
    pass CirclePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};