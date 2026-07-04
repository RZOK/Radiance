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
    const float distance = (1.0f / 1.1f);
    if (distanceFromCenter > distance - (2.0f / radius) && distanceFromCenter <= distance)
        colorOutput += color;
    else if (distanceFromCenter <= distance)
        colorOutput += color * 0.2f;
    return colorOutput;

}

technique Technique1
{
    pass CirclePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};