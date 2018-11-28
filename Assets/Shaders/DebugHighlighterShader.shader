Shader "Custom/Paint" 
 {
     Properties 
     {
         _MainTex ("Albedo (RGBA)", 2D) = "white" {}
         _PaintTex ("Paint Texture (RGBA)", 2D) = "white" {}
         _BlendAmount("Blend Amount", Range(0.0, 1.0)) = 1.0
		 _Transparency("Transparency", Range(0.0,1.0)) = 1.0
     }
     SubShader 
     {
		 Lighting Off
         Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
         LOD 100

		 ZWrite Off
		 Blend SrcAlpha OneMinusSrcAlpha
         
		 Pass {
         CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

		 #include "UnityCG.cginc"
 
         sampler2D _MainTex;
         sampler2D _PaintTex;
         fixed _BlendAmount;
 
         struct Input {
             float2 uv_MainTex;
             float2 uv_PaintTex;
         };
 
         void surf (Input IN, inout SurfaceOutput o) 
         {
             fixed4 main = tex2D (_MainTex, IN.uv_MainTex);
             fixed4 paint = tex2D (_PaintTex, IN.uv_PaintTex);
             o.Albedo = lerp(main.rgb, paint.rgb, paint.a * _BlendAmount);
         }
         ENDCG
     } 
	 }
     FallBack "Diffuse"
 }