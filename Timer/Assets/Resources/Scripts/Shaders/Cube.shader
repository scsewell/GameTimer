Shader "Custom/Cube"
{
	Properties
	{
		_NormalTex ("Albedo (RGB)", 2D) = "white" {}
		_CrazyTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Spec ("Spec", Color) = (1,1,1,1)
		_Spook ("Spook", Range(0,1)) = 0.0
	}
	SubShader
		{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf StandardSpecular fullforwardshadows
		#pragma target 3.0

		sampler2D _NormalTex;
		sampler2D _CrazyTex;

		struct Input
		{
			float2 uv_NormalTex;
		};

		half _Glossiness;
		half4 _Spec;
		fixed4 _Color;
		half _Spook;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			fixed4 norm = tex2D(_NormalTex, IN.uv_NormalTex) * (1 - _Spook);
			fixed4 crazy = tex2D(_CrazyTex, IN.uv_NormalTex) * _Spook;
			o.Albedo = norm.rgb + crazy.rgb;
			o.Specular = _Spec;
			o.Smoothness = _Glossiness;
			o.Emission = crazy.rgb * 5 + norm.rgb * 0.1f;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
