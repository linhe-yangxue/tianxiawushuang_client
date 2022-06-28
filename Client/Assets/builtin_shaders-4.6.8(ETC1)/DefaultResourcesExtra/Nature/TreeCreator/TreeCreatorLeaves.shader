Shader "GDE_Nature/Tree Creator Leaves" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_GlossMap ("Gloss (A)", 2D) = "black" {}
	_TranslucencyMap ("Translucency (A)", 2D) = "white" {}
	_ShadowOffset ("Shadow Offset (A)", 2D) = "black" {}
	
	// These are here only to provide default values
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.3
	_Scale ("Scale", Vector) = (1,1,1,1)
	_SquashAmount ("Squash", Float) = 1
	//1
	_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
	_UseSecondAlpha("Is Use Second Alpha", int) = 0
}

SubShader { 
	Tags { "IgnoreProjector"="True" "RenderType"="TreeLeaf" }
	LOD 200
		
CGPROGRAM
#pragma surface surf TreeLeaf alphatest:_Cutoff vertex:TreeVertLeaf addshadow nolightmap
#pragma exclude_renderers flash
#pragma glsl_no_auto_normalization
#include "Tree.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _GlossMap;
sampler2D _TranslucencyMap;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	fixed4 color : COLOR; // color.a = AO
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

void surf (Input IN, inout LeafSurfaceOutput o) {
	fixed4 c = tex2D_ETC1(_MainTex,_MainTex_Alpha, IN.uv_MainTex);
	o.Albedo = c.rgb * _Color.rgb * IN.color.a;
	o.Translucency = tex2D_ETC1(_TranslucencyMap,_MainTex_Alpha, IN.uv_MainTex).rgb;
	o.Gloss = tex2D_ETC1(_GlossMap,_MainTex_Alpha, IN.uv_MainTex).a;
	o.Alpha = c.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D_ETC1(_BumpMap,_MainTex_Alpha, IN.uv_MainTex));
}
ENDCG
}

Dependency "OptimizedShader" = "Hidden/Nature/Tree Creator Leaves Optimized"
FallBack "Diffuse"
}
