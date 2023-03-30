float4 color;
float radius;

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput = float4(0, 0, 0, 0);
    float2 mappedUv = float2(uv.x - 0.5, (1 - uv.y) - 0.5);
    float distanceFromCenter = length(mappedUv) * 2;
    float distance = 0.9f;
    
    if (distanceFromCenter > distance - (3 / radius) && distanceFromCenter < distance)
        colorOutput += color;
    return colorOutput;

}

technique Technique1
{
    pass CirclePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};