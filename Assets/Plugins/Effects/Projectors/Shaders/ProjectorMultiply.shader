Shader "Projector/AlphaBlend" {
	Properties{
		_ShadowTex("Cookie", 2D) = "white" {}
	_FalloffTex("FallOff", 2D) = "white" {}
	_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ProjectorDir("_ProjectorDir", Vector) = (0,0,0,0)
	}
		Subshader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Pass{
		ZWrite Off
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		LOD 200
		Offset -1, -1

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "UnityCG.cginc"

		fixed4 _Color;
		struct v2f {
		float4 uvShadow : TEXCOORD0;
		float4 uvFalloff : TEXCOORD1;
		UNITY_FOG_COORDS(2)
			float4 pos : SV_POSITION;
	};

		struct VertexInput {
			float4 vertex : POSITION;
			fixed3 normal : NORMAL;
		};

	float4x4 unity_Projector;
	float4x4 unity_ProjectorClip;
	fixed4 _ProjectorDir;

	v2f vert(VertexInput v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uvShadow = mul(unity_Projector, v.vertex);
		o.uvFalloff = mul(unity_ProjectorClip, v.vertex);
	//	o.Albedo = _Color.rgb;
	//	o.Emission = _Color.rgb; // * _Color.a;
		//o.Alpha = _Color.a;
		UNITY_TRANSFER_FOG(o, o.pos);
		o.uvFalloff.a = clamp(-1 * dot(normalize(_ProjectorDir), v.normal), 0, 1);
		return o;
	}


	sampler2D _ShadowTex;
	//sampler2D _FalloffTex;

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
	//fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
	//fixed4 res = texS * ceil(abs(i.uvFalloff.a));
	fixed4 res = texS;
//	res.a *= texF.a;
	//res.a *= a;
	return res;
	}
		ENDCG
	}
		//FallBack "Diffuse"
	}
}