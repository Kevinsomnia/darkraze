Shader "Depth Mask" {
	SubShader {		
		Tags {"Queue" = "Overlay+1" "IgnoreProjector"="True"}
			
		ColorMask 0
		ZWrite On
				
		Pass {}
	}
}