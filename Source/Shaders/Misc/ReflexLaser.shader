// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Darkraze/Reflex Laser" {
	Properties {
		_TintColor ("Laser Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Laser Texture", 2D) = "white" {}
	}

	Category {
		Tags { "Queue"="Overlay+10" "IgnoreProjector"="True" "RenderType"="Transparent" "IgnoreMotionBlur"="True"}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off
	
		SubShader {
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _TintColor;

				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
			
				float4 _MainTex_ST;

				v2f vert (v2f v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}
			
				fixed4 frag (v2f i) : COLOR {
					return i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				}
				ENDCG 
			}
		}	
	}
}