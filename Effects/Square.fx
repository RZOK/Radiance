float4 color;
float halfWidth;
float halfHeight;

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput = float4(0, 0, 0, 0);
    float xSize = 1.5 / halfWidth;
    float ySize = 1.5 / halfHeight;
    if ((uv.x < xSize || uv.x > 1 - xSize) || (uv.y < ySize || uv.y > 1 - ySize))
    {
        colorOutput = color;
    }
    return colorOutput;

}

technique Technique1
{
    pass SquarePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};