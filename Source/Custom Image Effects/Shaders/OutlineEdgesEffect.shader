// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Image Effects/OutlineEdgesEffect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_GrainTex ("Base (RGB)", 2D) = "gray" {}
	}

	SubShader {
		Pass {
			ZTest Always Fog {Mode Off}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct v2f { 
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 uvg : TEXCOORD1;
				};

				uniform sampler2D _MainTex;
				uniform sampler2D _GrainTex;
				uniform float4 _MainTex_TexelSize;
				uniform float _Threshold;
				uniform float _BackgroundBrightness;
				uniform float4 _ColorTint;
				uniform float _EdgeIntensity;
				uniform float _EdgeWidth;

				uniform float4 _GrainOffsetScale;
				uniform float _GrainIntensity;

				v2f vert (appdata_img v) {
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
					o.uvg = (v.texcoord.xy * _GrainOffsetScale.zw) + _GrainOffsetScale.xy;
					return o;
				}

				half4 frag(v2f i) : SV_Target {
					float offset = _MainTex_TexelSize * _EdgeWidth;
					half4 f00 = tex2D(_MainTex, i.uv + float2(-offset, -offset) + (_GrainOffsetScale.xy * 0.001));
					half4 f01 = tex2D(_MainTex, i.uv + float2(0, -offset) - (_GrainOffsetScale.xy * 0.0013));
					half4 f02 = tex2D(_MainTex, i.uv + float2(offset, -offset));
					half4 f03 = tex2D(_MainTex, i.uv + float2(-offset, 0));
					half4 f04 = tex2D(_MainTex, i.uv + float2(offset, 0) - (_GrainOffsetScale.xy * 0.0022));
					half4 f05 = tex2D(_MainTex, i.uv + float2(-offset, offset));
					half4 f06 = tex2D(_MainTex, i.uv + float2(0, offset));
					half4 sX = f00 - f02 + f03 - f04;
					half4 sY = f00 + f01 + f02 - (f05 * 2) - f06;
					half4 outline = sX * sX + sY * sY;

					half4 grain = (tex2D(_GrainTex, i.uvg) * 2 - 1) * _GrainIntensity;

					return ((dot(outline, _Threshold) * (0.3 + (grain * 0.7)) * 2.5 * _EdgeIntensity) + dot(f00, _BackgroundBrightness) + grain) * _ColorTint;
				}
			ENDCG
		}
	}

	Fallback Off
}