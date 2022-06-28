Shader "Easy/TwoPassTransparent" {
	Properties {
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGBA)", 2D) = "white" { }
		//1
		_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
		_UseSecondAlpha("Is Use Second Alpha", int) = 0
		_AddColor("Add Color", Color) = (0, 0, 0, 1)
	}

			CGINCLUDE
     
            #pragma vertex Vert
            #pragma fragment Frag
     
            #include "UnityCG.cginc"
      
            sampler2D 	_MainTex;           
            half4 		_Color;
            half4 		_AddColor;
            
            struct appdata {
				float4 vertex : POSITION;
				float2 txr1	: TEXCOORD0;
			};
         
            struct V2F         
            {         
                float4 pos:POSITION;    
                float2 txr1: TEXCOORD0;
            };
            
			//2
			sampler2D _MainTex_Alpha;
			//3
			fixed4 tex2D_ETC1(sampler2D sa,sampler2D sb,fixed2 v)        
			{                                                           
				fixed4 col = tex2D(sa,v);                                  
				fixed alp = tex2D(sb,v).r;                                 
				col.a = min(col.a,alp) ;                                   
				return col;   
			}
			        
            V2F Vert(appdata v)     
            {                 
                V2F output;
     
                output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                output.txr1 = v.txr1;
             
                return output;             
            }
             
         
            half4 Frag(V2F i):COLOR         
            {                 
                half4 o = tex2D_ETC1(_MainTex,_MainTex_Alpha, i.txr1);                                
                o.rgb = o.rgb*_AddColor.a + (o.rgb*_AddColor.rgb);  
                o *= _Color;
                                
                return o;
            }
         
            ENDCG

	SubShader {
		Tags { "Queue" = "Transparent" }

		Pass
		{
		
			Name "BASE"

			Cull Off
			Alphatest Greater 0
			ZWrite Off
			//ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha 
			//ColorMask RGB
			

			Lighting Off

			CGPROGRAM
     
            #pragma vertex Vert
            #pragma fragment Frag
     

         
            ENDCG
		}
		
		Pass
		{
			Name "BASE"
			ZWrite On
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha

			Lighting Off
			
			CGPROGRAM
     
            #pragma vertex Vert
            #pragma fragment Frag
         
            ENDCG
		}
	}
	Fallback "Diffuse"
}