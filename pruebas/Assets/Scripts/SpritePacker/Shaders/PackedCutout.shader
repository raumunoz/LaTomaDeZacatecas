Shader "SpritePacker/PackedCutout" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	AlphaTest Greater .5
	ColorMask RGB
	Cull back Lighting Off ZWrite Off
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				combine texture * primary
			}
		}
	}
}
}
