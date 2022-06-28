Shader "Game/NiceSprite" {

	Properties {
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_MainTex ("Base (RGB)", 2D) = "white" { }
		_MaskTex ("Base (RGB)", 2D) = "blank" { }
	}


	SubShader {
		Tags { "Queue" = "Transparent-110" }

		Pass
		{
			Name "BASE"
			ZWrite On
			//ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha

			Lighting Off

			CGPROGRAM
     
   
            #pragma vertex Vert
 
            #pragma fragment Frag
     
            #include "UnityCG.cginc"
     
 
            sampler2D _MainTex;
            sampler2D _MaskTex;
            //half4 _Color;
            
            struct appdata {
				float4 vertex : POSITION;
				//float3 normal : NORMAL;
				float2 txr1	: TEXCOORD0;
				float2 txr2	: TEXCOORD1;
			};
         
            struct V2F         
            {         
                float4 pos:POSITION;    
                float2 txr1: TEXCOORD0;
         		float2 txr2	: TEXCOORD1;
            };
               
         
            V2F Vert(appdata v)
     
            {
                 
                V2F output;
     
                output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
     
                output.txr1 = v.txr1;
                output.txr2 = v.txr2;
             
                return output;
             
            }
             
         
            half4 Frag(V2F i):COLOR         
            {                 
                half4 o = tex2D(_MainTex, i.txr1);                
                half4 mask = tex2D(_MaskTex, i.txr2);
                //o.a *= _Color.a;
                o.a *= mask.r * mask.g * mask.b * mask.a;
                return mask;

            }
 
         
            ENDCG
		}
	}
	Fallback "Diffuse"
}