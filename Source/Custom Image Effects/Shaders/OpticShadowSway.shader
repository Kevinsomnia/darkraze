Shader "Hidden/OpticShadowSway" {
	Properties {
		_MainTex ("Screen", 2D) = "" {}
		_Overlay ("Overlay Texture (RGB)", 2D) = "white" {}
		_ClampColor ("Clamp Color", Color) = (0, 0, 0, 0)
	}
	
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _Overlay;
			sampler2D _MainTex;

			half _ClampOffset;
			half4 _OverlayParams;
			half4 _MainTex_TexelSize;
			half4 _ClampColor;
			half _EyeReliefFactor;

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0.0)
					o.uv.y = 1.0 - o.uv.y;
				#endif
				return o;
			}

			half4 frag(v2f i) : SV_Target {
				half2 scale = _OverlayParams.zw / _EyeReliefFactor;
				half2 uvPos = (i.uv + half2(_OverlayParams.x + ((scale.x - 1.0) * 0.5), _OverlayParams.y + ((scale.y - 1.0) * 0.5))) / scale;

				if(uvPos.x < _ClampOffset || uvPos.x > 1.0 - _ClampOffset || uvPos.y < _ClampOffset || uvPos.y > 1.0 - _ClampOffset)
					return _ClampColor;
				
				half4 overlay = tex2D(_Overlay, uvPos);
				return lerp(tex2D(_MainTex, i.uv), overlay, overlay.a);
			}

			ENDCG
		}
	}

	Fallback Off
}