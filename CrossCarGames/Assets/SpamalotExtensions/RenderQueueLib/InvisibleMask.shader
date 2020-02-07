Shader "Custom/InvisibleMask" {
	SubShader {
		// Render after regular geometry, but before masked geometry
		Tags { "Queue" = "Geometry+10" }
		ColorMask 0
		ZWrite On
		Pass {}
	}
}