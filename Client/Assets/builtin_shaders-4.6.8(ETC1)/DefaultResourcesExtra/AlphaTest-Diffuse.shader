Shader "Transparent/Cutout/Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	//1
	_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
	_UseSecondAlpha("Is Use Second Alpha", int) = 0
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert
sampler2D _MainTex;
fixed4 _Color;
fixed _Cutoff;

struct Input {
	float2 uv_MainTex;
};

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

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D_ETC1(_MainTex,_MainTex_Alpha,IN.uv_MainTex) * _Color;
	clip(c.a - _Cutoff);
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Transparent/Cutout/VertexLit"
}
