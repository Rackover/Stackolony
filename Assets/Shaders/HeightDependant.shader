Shader "Custom/HeightDependentTint"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_HeightMin("Height Min", Float) = -1
		_HeightMax("Height Max", Float) = 1
		_Color0("Tint Color At 0", Color) = (0,0,0,1)
		_Color1("Tint Color At 1", Color) = (1,1,1,1)
		_Color2("Tint Color At 2", Color) = (1,1,1,1)
		_Color3("Tint Color At 3", Color) = (1,1,1,1)
		_Color4("Tint Color At 4", Color) = (1,1,1,1)
		_Color5("Tint Color At 5", Color) = (1,1,1,1)
		_Color6("Tint Color At 6", Color) = (1,1,1,1)
		_Color7("Tint Color At 7", Color) = (1,1,1,1)
		_Color8("Tint Color At 8", Color) = (1,1,1,1)
		_Color9("Tint Color At 9", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		CGPROGRAM
#pragma surface surf Lambert

		sampler2D _MainTex;
	fixed4 _Color0;
	fixed4 _Color1;
	fixed4 _Color2;
	fixed4 _Color3;
	fixed4 _Color4;
	fixed4 _Color5;
	fixed4 _Color6;
	fixed4 _Color7;
	fixed4 _Color8;
	fixed4 _Color9;
	float _HeightMin;
	float _HeightMax;

	struct Input
	{
		float2 uv_MainTex;
		float3 worldPos;
	};

	void surf(Input IN, inout SurfaceOutput o)
	{
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		float height = floor(IN.worldPos.y - _HeightMin);
		//float h = (_HeightMax - IN.worldPos.y) / (_HeightMax - _HeightMin);
		fixed4 tintColor = float4(0, 0, 0, 0);
		if (height == 1) {
			tintColor = _Color1;
		}
		else if (height == 2) {
			tintColor = _Color2;
		}
		else if (height == 3) {
			tintColor = _Color3;
		}
		else if (height == 4) {
			tintColor = _Color4;
		}
		else if (height == 5) {
			tintColor = _Color5;
		}
		else if (height == 6) {
			tintColor = _Color6;
		}
		else if (height == 7) {
			tintColor = _Color7;
		}
		else if (height == 8) {
			tintColor = _Color8;
		}
		else if (height == 9) {
			tintColor = _Color9;
		}
		else {
			tintColor = _Color0;
		}
		//fixed4 tintColor = float4(1, 0, 0, 1);
		//fixed4 tintColor = lerp(_ColorMax.rgba, _ColorMin.rgba, h);
		o.Albedo = c.rgb * tintColor.rgb;
		o.Alpha = c.a * tintColor.a;
	}
	ENDCG
	}
		Fallback "Diffuse"
}