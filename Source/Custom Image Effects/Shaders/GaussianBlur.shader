Shader "Hidden/Gaussian Blur" {
	Properties {
		_MainTex ("Screen (RGB)", 2D) = "" {}
	}

	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;

		uniform half4 _MainTex_TexelSize;
		uniform half _BlurSize;

		half4 frag_downsample(v2f_img i) : SV_Target {
			float2 d = _MainTex_TexelSize.xy * float2(1.0, -1.0);

			half4 color = tex2D(_MainTex, i.uv + d.xx);
			color += tex2D(_MainTex, i.uv + d.xy);
			color += tex2D(_MainTex, i.uv + d.yx);
			color += tex2D(_MainTex, i.uv + d.yy);
			return color * 0.25;
		}

		half4 gaussian_filter(float2 uv, float2 stride) {
			half4 color = tex2D(_MainTex, uv) * 0.227027027;

			float2 d = stride * 1.3846153846;
			color += tex2D(_MainTex, uv + d) * 0.3162162162;
			color += tex2D(_MainTex, uv - d) * 0.3162162162;
			
			d = stride * 3.2307692308;
			color += tex2D(_MainTex, uv + d) * 0.0702702703;
			color += tex2D(_MainTex, uv - d) * 0.0702702703;

			return color;
		}

		half4 frag_blur_h(v2f_img i) : SV_Target {
			return gaussian_filter(i.uv, float2(_MainTex_TexelSize.x * _BlurSize, 0.0));
		}

		half4 frag_blur_v(v2f_img i) : SV_Target {
			return gaussian_filter(i.uv, float2(0.0, _MainTex_TexelSize.y * _BlurSize));
		}
	ENDCG

	SubShader {
		ZTest Always Cull Off ZWrite Off
		
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_downsample
			ENDCG
		}

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_blur_h
			ENDCG
		}

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_blur_v
			ENDCG
		}
	}

	Fallback Off
}