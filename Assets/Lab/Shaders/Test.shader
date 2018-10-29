Shader "Custom/Test" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Thickness ("Thickness", Range(0,10)) = 0.0
	}
	SubShader {

		Tags  // Tags are a way of telling Unity certain properties of the shader we are writing.
		{
			// Don't write ; here

			"RenderType" = "Opaque"
			//	Opaque: most of the shaders (Normal, Self Illuminated, Reflective, terrain shaders).
			//	Transparent: most semitransparent shaders (Transparent, Particle, Font, terrain additive pass shaders).
			//	TransparentCutout: masked transparency shaders (Transparent Cutout, two pass vegetation shaders).
			//	Background: Skybox shaders.
			// 	Overlay: GUITexture, Halo, Flare shaders.
			// 	TreeOpaque: terrain engine tree bark.
			//	TreeTransparentCutout: terrain engine tree leaves.
			//	TreeBillboard: terrain engine billboarded trees.
			//	Grass: terrain engine grass.
			//	GrassBillboard: terrain engine billboarded grass.

			"Queue" = "Geometry"
			//	Background (1000): used for backgrounds and skyboxes,
			//	Geometry (2000): the default label used for most solid objects,
			//	Transparent (3000): used for materials with transparent properties, such glass, fire, particles and water;
			//	Overlay (4000): used for effects such as lens flares, GUI elements and texts.

			//Background+2, which indicates a queue value of 1002.
		}

		LOD 200 // Niveau de détail du shader

		CGPROGRAM // START SENDING DIRECTIVE TO THE GPU

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Alpha = c.a;
		}
		ENDCG // STOP SENDING DIRECTIVE TO THE GPU
	}
	FallBack "Diffuse"
}
