Shader "Hidden/Nature/Tree Creator Bark Optimized" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_BumpSpecMap ("Normalmap (GA) Spec (R)", 2D) = "bump" {}
	_TranslucencyMap ("Trans (RGB) Gloss(A)", 2D) = "white" {}
	
	// These are here only to provide default values
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Scale ("Scale", Vector) = (1,1,1,1)
	_SquashAmount ("Squash", Float) = 1

	//1
	_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
	_UseSecondAlpha("Is Use Second Alpha", int) = 0
}

SubShader { 
	Tags { "RenderType"="TreeBark" }
	LOD 200
	
CGPROGRAM
#pragma surface surf BlinnPhong vertex:TreeVertBark addshadow nolightmap
#pragma exclude_renderers flash
#pragma glsl_no_auto_normalization
#include "Tree.cginc"

sampler2D _MainTex;
sampler2D _BumpSpecMap;
sampler2D _TranslucencyMap;

struct Input {
	float2 uv_MainTex;
	fixed4 color : COLOR;
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
	fixed4 c = tex2D_ETC1(_MainTex,_MainTex_Alpha, IN.uv_MainTex);
	o.Albedo = c.rgb * _Color.rgb * IN.color.a;
	
	fixed4 trngls = tex2D_ETC1 (_TranslucencyMap,_MainTex_Alpha, IN.uv_MainTex);
	o.Gloss = trngls.a * _Color.r;
	o.Alpha = c.a;
	
	half4 norspc = tex2D_ETC1 (_BumpSpecMap,_MainTex_Alpha,IN.uv_MainTex);
	o.Specular = norspc.r;
	o.Normal = UnpackNormalDXT5nm(norspc);
}
ENDCG
}

Dependency "BillboardShader" = "Hidden/Nature/Tree Creator Bark Rendertex"
}
