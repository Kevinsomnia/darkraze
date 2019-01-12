// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Distort Effect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DispTex ("Displacement (RGB)", 2D) = "bump" {}
		_Intensity ("Glitch Intensity", Range(0.0, 1.0)) = 1
	}

	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _DispTex;
			uniform float _Intensity;
			uniform float _Scale;
			uniform float4 _Offset;
			uniform float _SplitPos;

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;

				return o;
			}

			half4 frag(v2f i) : COLOR {
				half4 normal = tex2D(_DispTex, (i.uv.xy + _Offset.xy) * _Scale);

				i.uv.xy += (normal.xy - 0.5) * _Intensity;

				if(i.uv.y < _SplitPos) {
					i.uv.x += _Offset.z;
				}
				else {
					i.uv.x -= _Offset.w;
				}

				half4 color = tex2D(_MainTex, i.uv.xy);
				return color;
			}
			ENDCG
		}
	}

	Fallback Off
}