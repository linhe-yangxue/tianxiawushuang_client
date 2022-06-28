
Shader "Hidden/Internal-Flare" 
{
	SubShader {

		Tags {"RenderType"="Overlay"}
		ZWrite Off ZTest Always
		Fog {Mode Off}
		Cull Off
		Blend One One
		ColorMask RGB

		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _FlareTexture;
			
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

			float4 _FlareTexture_ST;
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
				o.texcoord = TRANSFORM_TEX(v.texcoord, _FlareTexture);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D(_FlareTexture, i.texcoord) * i.color;
			}
			ENDCG 
		}
	}
}
