sampler2D starTexSampler : register(s0);
sampler2D itemTexSampler : register(s1);
float alpha;

float4 PixelShaderFunction(float2 uv : TEXCOORD, float4 Position : SV_Position) : COLOR0
{
    float4 starColor = tex2D(starTexSampler, uv);
    float4 itemColor = tex2D(itemTexSampler, uv);
    
    return itemColor * starColor * alpha;
}

technique Technique1
{
    pass StarColorPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};