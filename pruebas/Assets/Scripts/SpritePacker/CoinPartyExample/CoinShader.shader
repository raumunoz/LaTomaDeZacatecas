Shader "Coin/PackedCutout" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	//_Color ("_WorldSpaceMin assigned by script", Vector) = (1,1,1,1)
}
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	Zwrite off
	
	Pass {
		Name "BASE"
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
//float4 _Color;

struct v2f {
  float4 pos : SV_POSITION;
  fixed2 uv : TEXCOORD0;
  fixed4 color : COLOR;
};

v2f vert (appdata_full v) {
  v2f o;
  o.uv = v.texcoord.xy;
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.color = v.color;
  return o;
}

half4 frag (v2f i) : COLOR {
	fixed4 tex = tex2D(_MainTex, i.uv);
			
	// reconstruct red channel as linear combination of blue and green channels
	// these values were obtained trough trial and error by tweaking them as material parameters
	fixed3 albedo = fixed3((tex.g*1.54+tex.b*-0.52), tex.g, tex.b);
	
	// are we on the flat face of the coin or the side?
	// tex.b is 0 on the face and >= 0.1 on the side
	// saturate is similar to rounding to an int
	fixed side = saturate(tex.r*11);
	
	// first the highlight on the round edge
	// tex.b is 0.1..1 where 0.1 is bottom of coin and 1 is top
	// color.a is 0..1 where 0 is light under coin and 1 is light above coin
	fixed highlight = (1-abs(tex.r-i.color.a))*side;
	highlight = highlight+pow(highlight, 8)*2;
	// then the face - assume its flat.
	fixed faceHighlight = (1-side);

 	return fixed4(albedo * (i.color.rgb*4 * faceHighlight) + albedo * highlight, tex.a);
}
ENDCG
    }
 
}
} 