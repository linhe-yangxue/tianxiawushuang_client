Shader "Self-Illumin/Parallax Specular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_Parallax ("Height", Range (0.005, 0.08)) = 0.02
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
	_Illum ("Illumin (A)", 2D) = "white" {}
	_Illum_Alpha("Illum Trans (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_ParallaxMap ("Heightmap (A)", 2D) = "black" {}
	_ParallaxMap_Alpha ("Heightmap Trans(A)", 2D) = "black" {}
	_EmissionLM ("Emission (Lightmapper)", Float) = 0
	//1
	_UseSecondAlpha("Is Use Second Alpha", int) = 0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 600
	
CGPROGRAM
#pragma surface surf BlinnPhong
#pragma target 3.0

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _ParallaxMap;
sampler2D _ParallaxMap_Alpha;
sampler2D _Illum;
fixed4 _Color;
float _Parallax;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float2 uv_Illum;
	float3 viewDir;
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
	half h = tex2D_ETC1 (_ParallaxMap,_ParallaxMap_Alpha, IN.uv_BumpMap).w;
	float2 offset = ParallaxOffset (h, _Parallax, IN.viewDir);
	IN.uv_MainTex += offset;
	IN.uv_BumpMap += offset;
	IN.uv_Illum += offset;

	fixed4 tex = tex2D_ETC1(_MainTex,_MainTex_Alpha, IN.uv_MainTex);
	fixed4 c = tex * _Color;
	o.Albedo = c.rgb;
	o.Gloss = tex.a;
	o.Emission = c.rgb * tex2D_ETC1(_Illum,_Illum_Alpha, IN.uv_Illum).a;
	o.Specular = _Shininess;
	o.Alpha = c.a;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
}
FallBack "Self-Illumin/Bumped Specular"
}
