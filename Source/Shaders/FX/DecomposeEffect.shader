Shader "Darkraze/FX/Decompose Effect" {
    Properties {
      _Color ("Main Color", Color) = (1, 1, 1, 1)
      _MainTex ("Texture (RGB)", 2D) = "white" {}
      _SliceGuide ("Slice Guide (RGB)", 2D) = "white" {}
      _SliceAmount ("Slice Amount", Range(0.0, 1.0)) = 0.5
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      Cull Off
      CGPROGRAM
      #pragma surface surf Lambert addshadow
      struct Input {
          float2 uv_MainTex;
          float2 uv_SliceGuide;
          float _SliceAmount;
      };
      sampler2D _MainTex;
      sampler2D _SliceGuide;
      fixed4 _Color;
      float _SliceAmount;
      void surf (Input IN, inout SurfaceOutput o) {
          clip(tex2D (_SliceGuide, IN.uv_SliceGuide).rgb - _SliceAmount);
          o.Albedo = (tex2D(_MainTex, IN.uv_MainTex) * _Color).rgb;
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }