float4 color;
float maxProgress;

float4 PixelShaderFunction(float2 uv : TEXCOORD) : COLOR0
{
    uv.x -= uv.x % (1 / 240);
    uv.y -= uv.y % (1 / 240);
    
    float2 center = float2(0.5, 0.5);
    float2 between = uv - center;
    float len = length(between);
    
    if (len < 0.4 || len > 0.495)
        return float4(0, 0, 0, 0);
 
    float angle = atan2(between.x, between.y) + 3.1415926;
    float anglePercent = angle / 6.28318531;
    
    if (anglePercent > maxProgress || anglePercent > 0.8 || anglePercent < 0.2)
        return float4(0, 0, 0, 0);
    
    
    if (len < 0.415 || len > 0.48)
        return float4(0.25, 0.25, 0.25, color.w);
    if (len < 0.43 || len > 0.465)
        return float4(0.4, 0.4, 0.4, color.w);
    
    return color;

}

technique Technique1
{
    pass CirclePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};