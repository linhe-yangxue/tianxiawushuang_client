Shader "Unlit/Unlight"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "black" {}
		 //1
		_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
		_Mask("Mask",2D)="white"{}
		_Mask_Alpha("Mask_Alpha",2D) = "white"{}
		_ChangeColor("ChangeColor",Color) = (1,1,1,1)
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_UseSecondAlpha("Is Use Second Alpha", int) = 0
		
	}
	SubShader
	{
		Tags
		{
			 "Queue" = "AlphaTest"
			"IgnoreProjector" = "True"
			"RenderType" = "TransparentCutout"
		}
		Pass
		{
			Cull Off
			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _Mask;
			float4 _MainTex_ST;
			float4 _ChangeColor;
			float _Cutoff;
	
			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
			
			 //2
			  sampler2D _MainTex_Alpha;
			  sampler2D _Mask_Alpha;
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
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}
				
			fixed4 frag (v2f IN) : COLOR
			{
			    float4 c = tex2D_ETC1(_MainTex,_MainTex_Alpha, IN.texcoord);
			    clip(c.a-_Cutoff);
			    float4 mask = tex2D_ETC1(_Mask,_Mask_Alpha,IN.texcoord);
			    float4 final = c*mask*_ChangeColor;
			    return final;
				//return tex2D(_MainTex, IN.texcoord) * IN.color * tex2D(_Mask,IN.texcoord);
			}
			ENDCG
		}
	}
}
