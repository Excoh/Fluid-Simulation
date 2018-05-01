float4x4 World, View, Projection;
float4x4 WorldInverseTranpose;

Texture2D tex;

sampler samp = sampler_state
{
	Texture = <tex>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

struct VertexInput
{
	float4 position : SV_Position;
	float2 texcoord : TEXCOORD0;
};

struct VertexOutput
{
	float4 position : SV_Position;
	float2 texcoord : TEXCOORD0;
};

VertexOutput vertex(VertexInput input)
{
	VertexOutput OUT;

	float4 worldPos = mul(input.position, World);
	float4 viewPos = mul(worldPos, View);

	OUT.position = mul(viewPos, Projection);
	OUT.texcoord = input.texcoord;

	return OUT;
}

float4 fragment(VertexOutput input) : SV_Target
{

	float2 left = input.texcoord - float2((1.0 / 100), 0);
	float2 right = input.texcoord + float2((1.0 / 100), 0);
	float2 bottom = input.texcoord - float2(0, (1.0 / 100));
	float2 top = input.texcoord + float2(0, (1.0 / 100));

	float4 color;
	//if (left.x > 0 && right.x < 1 && top.y < 1 && bottom.y > 0)
	//{
	float4 fieldL = tex2D(samp, left);
	float4 fieldR = tex2D(samp, right);
	float4 fieldT = tex2D(samp, top);
	float4 fieldB = tex2D(samp, bottom);
	color = 0.5 * ((fieldR - fieldL) + (fieldT - fieldB));
	return color;
}

technique Main
{
	pass P1
	{
		VertexShader = compile vs_5_0 vertex();
		PixelShader = compile ps_5_0 fragment();
	}
}
