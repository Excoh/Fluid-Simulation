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
	float4 position : SV_Position;		// I think this is the coordinate grid
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
	float4 color;
	if (input.texcoord.x > 0.975 || input.texcoord.x < 0.025 || input.texcoord.y > 0.975 || input.texcoord.y < 0.025)
	{
		color = -1 * tex2D(samp, input.position * (1.0 / 100.0));
	}
	else color = float4(0,0,0,0);
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