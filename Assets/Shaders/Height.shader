 Shader "Custom/HeightDependentTint" 
 {
  Properties 
  {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _SlopeTex ("Base (RGB)", 2D) = "white" {}
    _HeightMin ("Height Min", Float) = -1
    _HeightMax ("Height Max", Float) = 1
    _ColorMin ("Tint Color At Min", Color) = (0,0,0,1)
    _ColorMax ("Tint Color At Max", Color) = (1,1,1,1)
    _Blend1 ("Blend between Base and Blend 1 textures", Range (0, 1) ) = 0 
  }
  
  SubShader
  {
    Tags { "RenderType"="Opaque" }
	  LOD 400
  
    CGPROGRAM
    #pragma surface surf Lambert
    #pragma surface surf BlinnPhong
    
    sampler2D _MainTex;
    sampler2D _SlopeTex;
    fixed4 _ColorMin;
    fixed4 _ColorMax;
    float _HeightMin;
    float _HeightMax;
    float _Blend1;
  
    struct Input
    {
      float2 uv_MainTex;
      float3 worldPos;
    };
    
    /*
    void vert (inout appdata_full v, out Input o) {
          UNITY_INITIALIZE_OUTPUT(Input,o);
          o.localPos = v.vertex.xyz;
    }
    */

    void surf (Input IN, inout SurfaceOutput o) 
    {
      half4 c = tex2D (_MainTex, IN.uv_MainTex);

      /*
      fixed4 mainCol = tex2D(_MainTex, IN.uv_MainTex);
      fixed4 texTwoCol = tex2D(_SlopeTex, IN.uv_MainTex);
      fixed4 output = lerp(mainCol, texTwoCol, _Blend1);
      output.rgb *
      output.a *
      */

      float h = (_HeightMax-IN.worldPos.y) / (_HeightMax-_HeightMin);
      fixed4 tintColor = lerp(_ColorMax.rgba, _ColorMin.rgba, h);

      o.Albedo = c.rgb * tintColor.rgb;
      o.Alpha = c.a * tintColor.a;
    }
    ENDCG
  } 
  Fallback "Diffuse"
}
