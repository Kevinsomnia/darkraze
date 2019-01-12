Shader "Darkraze/Shadow Only" {
    Properties {
        _Cutoff ("", Range(0,1)) = 0.5
    }
   
    SubShader {
        Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
 
        Pass {
            Name "Caster"
            Tags { "LightMode" = "ShadowCaster" }
            Offset 1, 1
           
            Fog {Mode Off}
            ZWrite On ZTest LEqual Cull Off
   
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_shadowcaster
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
               
                struct v2f {
                    V2F_SHADOW_CASTER;
                };
                              
                v2f vert( appdata_base v ) {
                    v2f o;
                    TRANSFER_SHADOW_CASTER(o)
                    return o;
                }
               
                uniform sampler2D _MainTex;
               
                float4 frag( v2f i ) : COLOR {
                    clip(0.5);
                    SHADOW_CASTER_FRAGMENT(i)
                }
            ENDCG
        }
    }

    Fallback Off
}
 