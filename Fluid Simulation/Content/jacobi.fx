float4x4 World, View, Projection;
float4x4 WorldInverseTranpose;

Texture2D tex;		// divergence
Texture2D pressure;	// custom pressure field affecting the fluid

sampler samp = sampler_state
{
	Texture = <tex>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

sampler p_samp = sampler_state
{
	Texture = <pressure>;
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

struct PixelOutput
{
	float4 velocity : COLOR0;
	float4 pressure : COLOR1;
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
	//float2 wat = input.texcoord.x + 1.0;
	// self and neighboring pressure
	float4 pTop = tex2D(p_samp, input.texcoord + float2(0.0, (1/100) ));
	float4 pBot = tex2D(p_samp, float2(input.texcoord.x, input.texcoord.y - (1.0 / 100.0)));
	float4 pLeft = tex2D(p_samp, float2(input.texcoord.x - (1.0 / 100.0), input.texcoord.y ));
	float4 pRight = tex2D(p_samp, float2(input.texcoord.x + (1.0 / 100.0), input.texcoord.y ));

	//float4 pc = tex2D(samp, input.texcoord);
	//float4 pTop = tex2D(samp, float2(input.texcoord.x, input.texcoord.y + (1.0 / 100.0)));
	//float4 pBot = tex2D(samp, float2(input.texcoord.x, input.texcoord.y - (1.0 / 100.0)));
	//float4 pLeft = tex2D(samp, float2(input.texcoord.x - (1.0 / 100.0), input.texcoord.y));
	//float4 pRight = tex2D(samp, float2(input.texcoord.x + (1.0 / 100.0), input.texcoord.y));

	float4 bc = tex2D(samp, input.texcoord);
	float4 color = (pLeft + pRight + pBot + pTop + 1.435 * bc) * 0.245;
//	if (input.texcoord.x > 0.975 || input.texcoord.x < 0.025 || input.texcoord.y > 0.975 || input.texcoord.y < 0.025) color = -tex2D(samp, input.texcoord);

	return color;
}

PixelOutput fragment2(VertexOutput input)
{
	PixelOutput OUT;
	// clears the pressure
	OUT.pressure = 0;
	OUT.velocity = tex2D(samp, input.texcoord);
	OUT.pressure = float4(0.0, 1.0, 0.0, 1.0);

	return OUT;
}

technique Main
{
	pass P1
	{
		VertexShader = compile vs_5_0 vertex();
		PixelShader = compile ps_5_0 fragment();
	}
}
