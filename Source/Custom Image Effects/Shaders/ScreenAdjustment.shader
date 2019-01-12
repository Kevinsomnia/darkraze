Shader "Hidden/Screen Adjustment" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _Brightness;
			uniform float _SaturationAmount;
			uniform float4 _ColorTint;
			uniform half4 _SelectiveVariables;
			uniform float _Smoothness;

			float4 frag(v2f_img i) : COLOR {
				float4 scrn = tex2D(_MainTex, i.uv) * _ColorTint;

				float3 LuminanceCoeff = float3(0.2126, 0.7152, 0.0722);
				float avgLumin = dot(scrn.rgb, LuminanceCoeff);
				
				if(avgLumin < _SelectiveVariables.x) {
					scrn.rgb *= 1.0 - (clamp((_SelectiveVariables.x - avgLumin) / _Smoothness, 0, 1) * _SelectiveVariables.y);
				}
				else if(avgLumin > _SelectiveVariables.z) {
					scrn.rgb *= 1.0 + (clamp((avgLumin - _SelectiveVariables.z) / _Smoothness, 0, 1) * _SelectiveVariables.w);
				}

				float3 intensity = avgLumin.rrr;
				scrn.rgb = lerp(intensity, scrn.rgb, _SaturationAmount);

				return scrn * _Brightness * _Brightness;
			}

			ENDCG
		}
	}

	Fallback Off
}