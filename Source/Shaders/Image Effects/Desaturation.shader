Shader "Hidden/Image Effects/Desaturation" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SaturationAmount ("Saturation Amount", Range(0.0, 1.0)) = 1.0
	}
	
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform float _SaturationAmount;
			
			float4 frag (v2f_img i) : COLOR
			{
				float4 renderTex = tex2D(_MainTex, i.uv);
				float3 LuminaceCoeff = float3(0.27,0.6,0.16);
				
				float intensityf = dot(renderTex.rgb, LuminaceCoeff);
				float3 intensity = float3(intensityf, intensityf, intensityf);
				
				renderTex.rgb = lerp(intensity, renderTex.rgb, _SaturationAmount);
				return renderTex * (0.92 + (_SaturationAmount * 0.08));
			}
			ENDCG
		}
	}
}