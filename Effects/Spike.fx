float4 color;
float2 startPos;
float thickness;
float2 endPos;
float threshold;
float scale;

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float2 p1 = startPos;
    float2 p2 = endPos;
    float4 p3 = Position;
    float2 p12 = p2 - p1;
    float2 p13 = p3.xy - p1;
    float4 colorOutput = float4(0, 0, 0, 0);
    uv.x = (uv.x / 2) + 0.5;  //the line that makes it from beam to spike
    float2 mappedUv = float2(uv.x - 0.5, (1 - uv.y) - 0.5);
    float distanceFromCenter = length(mappedUv) * 2;
    
    float d = dot(p12, p13) / length(p12);
    float2 p4 = p1 + normalize(p12) * d;
    if (length(p4 - p3.xy) < thickness
          && length(p4 - p1) <= length(p12)
          && length(p4 - p2) <= length(p12))
    {
        colorOutput += color;
    }
    colorOutput *= 1 - distanceFromCenter;
    return colorOutput;

}

technique Technique1
{
    pass SpikePass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};