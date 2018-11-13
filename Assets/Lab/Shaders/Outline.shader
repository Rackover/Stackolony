Shader "Stackolony/Outline"
{
	Properties
	{
		[Header(Runetime)][Space(1)]
		[Toggle] _Selected("Selected", Int) = 0

		[Space(2)][Header(Texture property)][Space(1)]
		_Color("Main Color", Color) = (.5, .5, .5, 1)
		_MainTex ("Texture", 2D) = "white" {}

		[Space(2)][Header(Outline property)][Space(1)]
		_OutlineColor("Outline color", Color) = (0, 0, 0 ,1)
		_OutlineWidth("Outline width", Range(1.0, 5.0)) = 1.01
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
	
	struct v2f
	{
		float4 pos : POSITION; 
		float3 normal : NORMAL;
	};

	float _OutlineWidth;
	float4 _OutlineColor;

	v2f vert(appdata v)
	{
		v.vertex.xyz *= _OutlineWidth;
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		return o;
	}

	ENDCG
	SubShader
	{
		Tags { "Queue"="Transparent" }
		LOD 200

		Pass // RENDERING THE OUTLINE
		{
			ZWrite Off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) : COLOR
			{
				return _OutlineColor;
			}
			ENDCG
		}	

		Pass // RENDERING THE OBJECT
		{
			ZWrite On
			
			Material
			{
				Diffuse[_Color]
				Ambient[_Color]
			}

			Lighting On
		
			SetTexture[_MainTex]
			{
				ConstantColor[_Color]
			}

			SetTexture[_MainTex]
			{
				Combine previous * primary DOUBLE
			}
		}
	}
}
