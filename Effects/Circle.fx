float4 color;
float radius;
float2 resolution;
bool pixelate;

float2 Resolution;

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 colorOutput = float4(0, 0, 0, 0);
    float2 mappedUv = float2(uv.x - 0.5, (1 - uv.y) - 0.5);
    if (pixelate)
    {
        mappedUv.x -= mappedUv.x % (1 / resolution.x);
        mappedUv.y -= mappedUv.y % (1 / resolution.y);
    }
    float distanceFromCenter = length(mappedUv) * 2;
    float distance = 0.93f;
    
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