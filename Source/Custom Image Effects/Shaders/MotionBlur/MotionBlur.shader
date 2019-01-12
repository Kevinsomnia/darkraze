Shader "Hidden/Motion Blur" {
	Properties {
		_MainTex ("", 2D) = "" {}
		_VelocityTex ("", 2D) = "" {}
		_NeighborMaxTex ("", 2D) = "" {}
	}

	CGINCLUDE
	#include "MotionBlur.cginc"
	#pragma target 3.0
	ENDCG

	SubShader {
		ZTest Always Cull Off ZWrite Off

		// Pass 0: Velocity texture setup.
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_VelocitySetup
			ENDCG
		}

		// Pass 1: TileMax filter (2 pixels width with normalization)
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_TileMax1
            ENDCG
        }

		// Pass 2: TileMax filter (2 pixels width)
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_TileMax2
            ENDCG
        }

		// Pass 3: TileMax filter (variable width)
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_TileMaxV
            ENDCG
        }

		// Pass 4: NeighborMax filter
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_NeighborMax
            ENDCG
        }

		// Pass 5: Reconstruction filter.
		Pass {
			CGPROGRAM
			#pragma vertex vert_Multitex
			#pragma fragment frag_Reconstruction
			ENDCG
		}
	}
}