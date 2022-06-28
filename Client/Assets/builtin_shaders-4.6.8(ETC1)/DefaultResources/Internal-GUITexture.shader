
Shader "Hidden/Internal-GUITexture" 
{
	Properties { _MainTex ("Texture", any) = "" {} } 

	SubShader {

		Tags { "ForceSupported" = "True" "RenderType"="Overlay" } 
		
		Lighting Off 
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off } 
		ZTest Always 
		
		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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

			sampler2D _MainTex;

			uniform float4 _MainTex_ST;
			//2				
			uniform sampler2D _MainTex_Alpha;
			uniform float _UseSecondAlpha;
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
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return 2.0f * tex2D(_MainTex, i.texcoord) * i.color;
			}
			ENDCG 
		}
	} 
	
	Fallback off 
}
