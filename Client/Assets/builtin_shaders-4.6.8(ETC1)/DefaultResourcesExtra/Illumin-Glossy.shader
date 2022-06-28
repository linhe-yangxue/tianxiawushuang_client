Shader "Self-Illumin/Specular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
	_Illum ("Illumin (A)", 2D) = "white" {}
	_Illum_Alpha("Illum Trans (A)", 2D) = "white" {}
	_EmissionLM ("Emission (Lightmapper)", Float) = 0

	//1
	_UseSecondAlpha("Is Use Second Alpha", int) = 0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 300
	
CGPROGRAM
#pragma surface surf BlinnPhong

sampler2D _MainTex;
sampler2D _Illum;
fixed4 _Color;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float2 uv_Illum;
};

//2
sampler2D _MainTex_Alpha;
sampler2D _Illum_Alpha;
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
	fixed4 tex = tex2D_ETC1(_MainTex,_MainTex_Alpha,IN.uv_MainTex);
	fixed4 c = tex * _Color;
	o.Albedo = c.rgb;
	o.Emission = c.rgb * tex2D_ETC1(_Illum,_Illum_Alpha,IN.uv_Illum).a;
	o.Gloss = tex.a;
	o.Alpha = c.a;
	o.Specular = _Shininess;
}
ENDCG
}
FallBack "Self-Illumin/Diffuse"
}
