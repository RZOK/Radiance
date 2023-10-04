float hQuish;
float vQuish;
float4 drawColor;
texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

float GetLerpValue(float from, float to, float t)
{
    return (t - from) / (to - from);
}

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float leftBound = hQuish;
    float rightBound = 1 - hQuish;
    float topBound = vQuish;
    float bottomBound = 1 - vQuish;
    if(uv.x < leftBound || uv.x > rightBound || uv.y < topBound || uv.y > bottomBound)
        return float4(0, 0, 0, 0);
    
    float2 unquishedUV = float2(GetLerpValue(leftBound, rightBound, uv.x), GetLerpValue(topBound, bottomBound, uv.y));
    float4 color = tex2D(samplerTex, unquishedUV);
    
    return color * drawColor;

}

technique Technique1
{
    pass QuishPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};