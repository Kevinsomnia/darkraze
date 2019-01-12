Shader "Self-Illumin/Reflective/Bumped Specular" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_MainTex ("Base (RGB) RefStrGloss (A)", 2D) = "white" {}
		_Cube ("Reflection Cubemap", Cube) = "" { TexGen CubeReflect }
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Illum ("Illumin (A)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 3.0
		#pragma exclude_renderers d3d11_9x

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Illum;
		samplerCUBE _Cube;

		fixed4 _Color;
		fixed4 _ReflectColor;
		half _Shininess;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_Illum;
			float3 worldRefl;
			INTERNAL_DATA
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c = tex * _Color;
			o.Albedo = c.rgb;
	
			o.Gloss = tex.a;
			o.Specular = _Shininess;
	
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	
			float3 worldRefl = WorldReflectionVector (IN, o.Normal);
			fixed4 reflcol = texCUBE (_Cube, worldRefl);
			reflcol *= tex.a;
			o.Emission = reflcol.rgb * _ReflectColor.rgb + (c.rgb * tex2D(_Illum, IN.uv_Illum).rgb * 2);
			o.Alpha = reflcol.a * _ReflectColor.a;
		}
		ENDCG
	}

	FallBack "Self-Illumin/VertexLit"
}