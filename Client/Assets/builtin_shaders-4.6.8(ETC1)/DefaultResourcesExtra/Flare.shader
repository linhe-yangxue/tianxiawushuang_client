Shader "FX/Flare" {
Properties {
	_MainTex ("Particle Texture", 2D) = "black" {}
	//1
	_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
	_UseSecondAlpha("Is Use Second Alpha", int) = 0
}
SubShader {
	Tags {
		"Queue"="Transparent"
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
		"PreviewType"="Plane"
	}
	Cull Off Lighting Off ZWrite Off Ztest Always Fog { Mode Off }
	Blend One One

	Pass {	
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 _TintColor;
		
		struct appdata_t {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		float4 _MainTex_ST;
		
		//2
		sampler2D _MainTex_Alpha;
		int _UseSecondAlpha;
		//3
		fixed4 tex2D_ETC1(sampler2D sa,sampler2D sb,fixed2 v)        
 		{                                                           
 			fixed4 col = tex2D(sa,v);                                  
 			fixed alp = tex2D(sb,v).r;                                 
 			col.a = min(col.a,alp) ;                                   
			return col;                                                
 		}   

		v2f vert (appdata_t v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.color = v.color;
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			fixed4 col;
			fixed4 tex = tex2D(_MainTex, i.texcoord);
			col.rgb = i.color.rgb * tex.rgb;
			col.a = tex.a;
			return col;
		}
		ENDCG 
	}
} 	

}
