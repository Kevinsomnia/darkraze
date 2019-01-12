// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blur And Flares" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f {
		half4 pos : POSITION;
		half2 uv : TEXCOORD0;
	};

	struct v2f_blur {
		half4 pos : POSITION;
		half2 uv : TEXCOORD0;
		half4 uv01 : TEXCOORD1;
		half4 uv23 : TEXCOORD2;
		half4 uv45 : TEXCOORD3;
	};
	
	half4 _Offsets;
	half4 _MainTex_TexelSize;
	
	sampler2D _MainTex;
		
	v2f vert (appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv =  v.texcoord.xy;
		return o;
	}

	v2f_blur vertWithMultiCoords (appdata_img v) {
		v2f_blur o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = v.texcoord.xy;
		o.uv01 =  v.texcoord.xyxy + _Offsets.xyxy * half4(1,1, -1,-1);
		o.uv23 =  v.texcoord.xyxy + _Offsets.xyxy * half4(1,1, -1,-1) * 2.0;
		o.uv45 =  v.texcoord.xyxy + _Offsets.xyxy * half4(1,1, -1,-1) * 3.0;
		return o;  
	}

	half4 fragGaussBlur (v2f_blur i) : COLOR {
		half4 color = half4 (0,0,0,0);
		color += 0.2 * tex2D (_MainTex, i.uv);
		color += 0.15 * tex2D (_MainTex, i.uv01.xy);
		color += 0.15 * tex2D (_MainTex, i.uv01.zw);
		color += 0.1 * tex2D (_MainTex, i.uv23.xy);
		color += 0.1 * tex2D (_MainTex, i.uv23.zw);
		color += 0.05 * tex2D (_MainTex, i.uv45.xy);
		color += 0.05 * tex2D (_MainTex, i.uv45.zw);	
		return color;
	}

	ENDCG
	
Subshader {
	 ZTest Always Cull Off ZWrite Off
	 Fog { Mode off } 

	 Pass {     

		  CGPROGRAM
      
		  #pragma fragmentoption ARB_precision_hint_fastest
		  #pragma exclude_renderers flash
		  #pragma vertex vertWithMultiCoords
		  #pragma fragment fragGaussBlur
      
		  ENDCG
	  } 
}
Fallback off
}
