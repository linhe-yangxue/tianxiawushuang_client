Shader "Projector/MouseCoord" {
   Properties {
      _ShadowTex ("Cookie", 2D) = "gray" { TexGen ObjectLinear }
      //_FalloffTex ("FallOff", 2D) = "white" { TexGen ObjectLinear   }
   }

   Subshader {
      Tags { "RenderType"="Transparent-1" }
      Pass {
         ZWrite Off
         Fog { Color (0, 0, 0) }
         //AlphaTest Greater 0
         ColorMask RGB
         Blend SrcAlpha OneMinusSrcAlpha
         //Blend SrcColor OneMinusSrcColor
		 Offset -1, -1
         SetTexture [_ShadowTex] {
            //combine texture, ONE - texture
            Matrix [_Projector]
         }
         //SetTexture [_FalloffTex] {
         //   constantColor (1,1,1,0)
         //   combine previous lerp (texture) constant
         //   Matrix [_ProjectorClip]
         //}
         
            //        CGPROGRAM
     
   
            //#pragma vertex Vert
 
            //#pragma fragment Frag
     
            //#include "UnityCG.cginc"
     
 
            //sampler2D _ShadowTex;
            //half4 _Color;
            //float4x4 _Projector;
         
            //struct V2F
         
            //{
         
            //    float4 pos:POSITION;
     
            //    float2 txr1:TEXCOORD0;
         
            //};
               
         
            //V2F Vert(appdata_base v)
     
            //{
                 
            //    V2F output;
     
            //    output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
     
            //    output.txr1 = mul(_Projector, v.texcoord);
             
            //    return output;
             
            //}
             
         
            //half4 Frag(V2F i):COLOR
         
            //{
                 
            //    half4 o = tex2D(_ShadowTex, i.txr1);                
            //    //o.a *= _Color.a;
            //    o = float4(1, 1, 1, 1);
            //    return o;
            //    //o.a = _Color.a;
            //    //return o;
            //}
 
         
            //ENDCG
         
      }
   }
}
