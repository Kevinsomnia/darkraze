// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Darkraze/Misc/Bumped Refraction" {
	Properties {
		_Refraction ("Refraction", Float) = 2
		_BumpMap ("Normal-map", 2D) = "bump" {}
	}

	Category {
		Tags {"Queue"="Transparent" "RenderType"="Opaque"}

		SubShader {
			GrabPass {							
				Name "BASE"
				Tags { "LightMode" = "Always" }
	 		}
	 		
			Pass {
				Name "BASE"
				Tags { "LightMode" = "Always" }
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float2 uvbump : TEXCOORD1;
				};

				float _Refraction;
				float4 _BumpMap_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;
					o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpMap );
					return o;
				}

				sampler2D _GrabTexture;
				float4 _GrabTexture_TexelSize;
				sampler2D _BumpMap;

				half4 frag( v2f i ) : COLOR {
					half2 bump = UnpackNormal(tex2D( _BumpMap, i.uvbump )).rg;
					float2 offset = bump * _Refraction * 30 * _GrabTexture_TexelSize.xy;
					i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
					
					half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
					return col;
				}
				ENDCG
			}
		}

		// Fallback for older cards and Unity non-Pro
		SubShader {
			Blend DstColor Zero
			Pass {
				Name "BASE"
				SetTexture [_MainTex] {	combine texture }
			}
		}
	}
}