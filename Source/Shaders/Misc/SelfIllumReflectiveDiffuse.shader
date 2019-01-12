Shader "Self-Illumin/Reflective/Diffuse" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
		_Illum ("Illumin (A)", 2D) = "white" {}
		_EmissionLM ("Emission (Lightmapper)", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
	
		CGPROGRAM
		#pragma surface surf Lambert
		
		sampler2D _MainTex;
		sampler2D _Illum;
		samplerCUBE _Cube;
		
		fixed4 _Color;
		fixed4 _ReflectColor;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_Illum;
			float3 worldRefl;
		};
		
		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c = tex * _Color;
			o.Albedo = c.rgb;
			
			fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
			o.Emission = reflcol.rgb * _ReflectColor.rgb + (c.rgb * tex2D(_Illum, IN.uv_Illum).rgb);
			o.Alpha = reflcol.a * _ReflectColor.a;
		}
		ENDCG
	}
	FallBack "Self-Illumin/VertexLit"
}