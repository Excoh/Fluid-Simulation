float4x4 World, View, Projection;
float4x4 WorldInverseTranpose;

float textureHeight;
float textureWidth;

Texture2D tex;
//Texture2D velocity;

sampler samp = sampler_state
{
	Texture = <tex>;
	magfilter = POINT;
	minfilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};
//
//sampler samp_vel = sampler_state
//{
//	Texture = <velocity>;
//	magilfter = LINEAR;
//	minfilter = LINEAR;
//	AddressU = CLAMP;
//	Address V = CLAMP;
//};

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
	float2 u = tex2D(samp, input.position * (1.0/100.0)).xy;
	float2 coord = (input.texcoord - 0.0015 * u);
	
	float color = tex2D(samp, coord);

	// boundary check
	if (input.texcoord.x > 0.975 || input.texcoord.x < 0.025 || input.texcoord.y > 0.975 || input.texcoord.y < 0.025) color = float4(0.15,0.15,0.15,1.0);

	
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
