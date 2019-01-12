Shader "Darkraze/Misc/Bullet Hole Decal" {
	Properties {
		_Color ("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_HeatGlow ("Heat Glow", Range(0, 1)) = 1
	}

	SubShader {
		Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Lighting Off
		ZTest LEqual
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0.01
		Offset -1, -1

		CGPROGRAM
		#pragma surface surf Lambert alpha:blend

		uniform sampler2D _MainTex;
		uniform fixed4 _Color;
		uniform float _HeatGlow;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			half3 intensity = half3(0, 0, 0);
			if(_HeatGlow > 0) {
				half2 center = half2(0.52, 0.49) - IN.uv_MainTex.xy;
				half dist = max(0, (0.05 + (_HeatGlow * 0.04)) - (sqrt(center.x * center.x + center.y * center.y) * 0.8));
				intensity = half3(1.0 + _HeatGlow * 0.5, 0.25 + _HeatGlow * 0.1, 0) * dist * 2.5;
			}

			o.Albedo = tex.rgb + intensity;
			o.Emission = intensity * 10 * _HeatGlow;
			o.Alpha = tex.a;
		}
		ENDCG
	}
}