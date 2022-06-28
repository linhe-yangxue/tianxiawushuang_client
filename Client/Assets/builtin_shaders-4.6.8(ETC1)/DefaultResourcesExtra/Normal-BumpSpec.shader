Shader "Bumped Specular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	//1
  _MainTex_Alpha ("Trans (A)", 2D) = "white" {}
  _UseSecondAlpha("Is Use Second Alpha", int) = 0
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
CGPROGRAM
#pragma surface surf BlinnPhong


sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
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
	fixed4 tex = tex2D_ETC1(_MainTex,_MainTex_Alpha,IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a;
	o.Alpha = tex.a * _Color.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap,IN.uv_BumpMap));
}
ENDCG
}

FallBack "Specular"
}
