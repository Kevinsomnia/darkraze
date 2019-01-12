Shader "Hidden/Radial Blur" {
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
			uniform half _BlurWidth;
			uniform half _BlurIntensity;
			uniform half _FisheyeEffect;

			float rand(float2 co) {
				return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 4758.5453);
			}

			half4 frag (v2f_img i) : COLOR {
				half2 coords = i.uv;
				coords = (coords - 0.5) * 2.0;		
		
				half2 realCoordOffs;
				realCoordOffs.x = (1-coords.y * coords.y) * _FisheyeEffect * (coords.x); 
				realCoordOffs.y = (1-coords.x * coords.x) * _FisheyeEffect * (coords.y);

				half4 screen = tex2D(_MainTex, i.uv - realCoordOffs);
				half4 additive = screen;

				half2 dir = half2(0.5, 0.5) - i.uv;
				half dist = sqrt(dir.x * dir.x + dir.y * dir.y);
				half effect = clamp(dist * _BlurIntensity, 0.2, 1.0);

				dir /= dist;
				
				for(int n = 0; n < 7; n++) {
					additive += tex2D(_MainTex, i.uv - realCoordOffs + half2(rand(i.uv) * 0.0012, rand(i.uv) * 0.0012) + (dir * _BlurWidth * dist * (-0.01 + (0.0033 * n))));
				}

				additive *= half4(1.18, 1.05, 1.1, 1.0);
				additive /= 7;

				return lerp(screen, additive, effect);
			}

			ENDCG
		}
	}
}