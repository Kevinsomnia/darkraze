// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "Darkraze/FX/Wet Surface" { 
	Properties {
		_BaseColor ("Base Water Color", Color) = (0, 0, 0)
		_WaveScale ("Wave Scale", Range (0.02,0.15)) = 0.063
		_WaterOpacity ("Water Opacity", Range (0, 1)) = 0.1
		_ReflIntensity ("Reflection Intensity", Range(0, 1)) = 1
		_ReflDistort ("Reflection Distortion", Range (0,1.5)) = 0.44
		_ReflBright ("Reflection Brightness", Range (1, 2)) = 1.0
		_BumpMap ("Bump-map", 2D) = "bump" {}
		_WaveSpeed ("Wave Speed", Vector) = (5, 4, -3, -5)
		_ReflectiveColor ("Reflective Color (RGB) Fresnel (A)", 2D) = "" {}
	}

	Subshader {
		Tags {"RenderType"="Transparent" "IgnoreProjector"="True" "Queue" = "Transparent-1"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"
			
			uniform half3 _BaseColor;
			uniform float4 _WaveScale4;
			uniform float4 _WaveOffset;
			uniform float _ReflDistort;
			uniform half _WaterOpacity;
			uniform half _ReflIntensity;
			uniform half _ReflBright;
			
			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 ref : TEXCOORD0;
				float2 bumpuv0 : TEXCOORD1;
				float2 bumpuv1 : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
			};
			
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				
				float4 temp;
				temp.xyzw = v.vertex.xzxz * _WaveScale4 * 50 / 1.0 + _WaveOffset;
				o.bumpuv0 = temp.xy;
				o.bumpuv1 = temp.wz;
				
				o.viewDir.xzy = ObjSpaceViewDir(v.vertex);
				
				o.ref = ComputeScreenPos(o.pos);
				
				return o;
			}
			
			sampler2D _ReflectionTex;
			sampler2D _ReflectiveColor;
			sampler2D _BumpMap;
			
			half4 frag( v2f i ) : COLOR
			{
				i.viewDir = normalize(i.viewDir);
				
				half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
				half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
				half3 bump = (bump1 + bump2) * 0.4;
				
				half fresnelFac = dot( i.viewDir, bump );
								
				float4 uv1 = i.ref; uv1.xy += bump * _ReflDistort;
				half4 refl = tex2Dproj( _ReflectionTex, UNITY_PROJ_COORD(uv1) );
				
				half4 color;
				
				half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
				color.rgb = lerp(water.rgb, refl.rgb * _BaseColor * 8 * _ReflBright * _ReflIntensity, water.a );
				color.a = _WaterOpacity;
				
				return color;
			}
			
		ENDCG
		}
	}
	
	Fallback "Transparent/Diffuse"
}
