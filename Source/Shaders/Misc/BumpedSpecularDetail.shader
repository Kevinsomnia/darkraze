Shader "Darkraze/Bumped Specular Detail" {
	Properties {
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.1
		_MainTex ("Base Texture (RGB)", 2D) = "white" {}
		_BumpMap ("Normal-map", 2D) = "bump" {}
		_Detail ("Detail Texture #1 (RGB)", 2D) = "gray" {}
		_DetailTwo ("Detail Texture #2 (RGB)", 2D) = "gray" {}
	}
	
	SubShader {
		Tags {"RenderType" = "Opaque"}
		LOD 300
	
		CGPROGRAM
		#pragma surface surf BlinnPhong
		
		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Detail;
		sampler2D _DetailTwo;
		fixed4 _Color;
		half _Shininess;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_Detail;
			float2 uv_DetailTwo;
		};
		
		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c = tex * _Color;
			c.rgb *= tex2D(_Detail, IN.uv_Detail).rgb * 2;
			c.rgb *= tex2D(_DetailTwo, IN.uv_DetailTwo).rgb * 2;
			o.Albedo = c.rgb;
			o.Gloss = tex.a * 10 * tex.r;
			o.Alpha = c.a;
			o.Specular = _Shininess;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		
		ENDCG
	}
	
	Fallback "Diffuse"
}