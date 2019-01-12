// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Darkraze/FX/Laser Pointer" {
    Properties {
    	_Color ("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white"
        _NoiseTex ("Noise Tex", 2D) = "white" 
    }
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	sampler2D _NoiseTex;
	
	half4 _Color;
	half4 _MainTex_ST;
	half4 _NoiseTex_ST;
								
	struct v2f {
		half4 pos : SV_POSITION;
		half4 uv : TEXCOORD0;
	};

	v2f vert(appdata_full v)
	{
		v2f o;
		
		o.pos = UnityObjectToClipPos (v.vertex);	
		o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.uv.zw = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				
		return o; 
	}
		
	fixed4 frag( v2f i ) : COLOR
	{	
		return tex2D (_MainTex, i.uv.xy) * tex2D (_NoiseTex, i.uv.zw) * _Color;
	}

	ENDCG
	
	SubShader {
		Tags {"Queue" = "Transparent-1"}
		Cull Off
		ZWrite Off
		ZTest LEqual
		Alphatest Greater 0
		Offset -1, -1
		Blend SrcAlpha One
		
		Pass {
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			ENDCG
		}
				
	} 
	FallBack Off
}
