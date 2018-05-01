float4x4 World, View, Projection;
float4x4 WorldInverseTranpose;

float2 Point;

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
	float4 color = tex2D(samp, input.texcoord);
	float d = distance(Point, input.texcoord);
	if (d < 0.05)
	{
		color += float4(1.0, 1.0, 1.0, 1.0);
	}
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