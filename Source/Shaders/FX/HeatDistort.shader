// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Darkraze/FX/Heat Distortion" {
	Properties {
		_TintColor("Distortion (A)", Color) = (1, 1, 1, 0.25)
		_BumpMap("Normalmap", 2D) = "bump" {}
	}

	CGINCLUDE
	#pragma fragmentoption ARB_precision_hint_fastest
	#pragma fragmentoption ARB_fog_exp2
	#include "UnityCG.cginc"

	sampler2D _GrabTexture : register(s0);
	float4 _GrabTexture_TexelSize;
	float4 _TintColor;
	sampler2D _BumpMap : register(s1);

	struct v2f {
		float4 vertex : POSITION;
		float4 uvgrab : TEXCOORD0;
		float2 uvbump : TEXCOORD1;
	};

	half4 frag(v2f i) : COLOR {
		half2 bump = UnpackNormal(tex2D(_BumpMap, i.uvbump)).rg;
		float2 offset = bump * _TintColor.a * 128 * _GrabTexture_TexelSize.xy;
		float depth = i.uvgrab.b;
		i.uvgrab.rg += (offset * depth);

		half4 col = tex2Dproj(_GrabTexture, i.uvgrab.rga);
		return col;
	}

	ENDCG

	Category {
		Tags {"Queue" = "Transparent+100" "RenderType" = "Transparent"}
		Fog {Mode Off}
		
		SubShader {
			GrabPass {
				NAME "BASE"
				Tags {"LightMode" = "Always"}
			}

			Pass {
				Name "BASE"
				Tags {"LightMode" = "Always"}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
				};

				v2f vert(appdata_t v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;
					o.uvbump = MultiplyUV( UNITY_MATRIX_TEXTURE1, v.texcoord );
					return o;
				}
				ENDCG
			}
		}

		//Fallback for no support
		SubShader {
			Blend DstColor Zero
			Pass {
				Name "BASE"
				SetTexture[_MainTex] { combine texture }
			}
		}
	}
}