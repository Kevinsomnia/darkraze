Shader "Hidden/Directional Blur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
       
			CGPROGRAM   
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
 
			#include "UnityCG.cginc"
 
			uniform sampler2D _MainTex;
			uniform sampler2D _CameraDepthTexture;
			uniform half _BlurAmount;
			uniform half4 _DirVect;
 
			half4 frag (v2f_img i) : COLOR {
				half4 color = tex2D(_MainTex, i.uv);
				half4 additive = color;

				int iterations = 11;
				for(int n = 1; n < iterations; n++)
				{
					additive += tex2D(_MainTex, i.uv + _DirVect * n * -0.001);
				}

				additive /= iterations;

				return lerp(color, additive, _BlurAmount);
			}

			ENDCG
		}
	} 
}