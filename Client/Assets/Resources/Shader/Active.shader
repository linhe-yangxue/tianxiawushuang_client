// ©2012 Hedgehog Team, do not sale. 

Shader "Active/Rim" { 
    Properties { 
      _MainTex ("Base (RGBA)", 2D) = "white" {} 
      _RimColor ("Rim Color", Color) = (0,0,0,0.0) 
      _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0    	  
   	  _AddColor ("Add Color", Color) = (0,0,0,0)

	  //1
		_MainTex_Alpha ("Trans (A)", 2D) = "white" {}
		_UseSecondAlpha("Is Use Second Alpha", int) = 0
    } 
    SubShader { 
      Tags { "RenderType" = "Opaque" } 
     
      
      CGPROGRAM 
       
       #pragma surface surf SimpleSpecular 
   float _Shininess; 
    
      half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) { 
          half3 h = normalize (lightDir + viewDir); 
          half diff = max (0, dot (s.Normal, lightDir)); 
          //float nh = max (0, dot (s.Normal, h)); 
          //float spec = pow (nh, 48.0); 
          //+ _LightColor0.rgb * spec * s.Alpha * _Shininess * _SpecColor
          half4 c; 
          c.rgb = (s.Albedo * _LightColor0.rgb * diff) * (atten * 2); 
          c.a = s.Alpha;           
          return c; 
      } 
             
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

      struct Input { 
          float2 uv_MainTex; 
          float3 viewDir; 
      }; 
      sampler2D _MainTex; 
      float4 _RimColor; 
      float _RimPower; 
      float4 _AddColor;
       
      void surf (Input IN, inout SurfaceOutput o) { 
      		float4 texC = tex2D_ETC1 (_MainTex,_MainTex_Alpha, IN.uv_MainTex);
          o.Albedo = texC.rgb; 
          //o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap)); 
          half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal)); 
          o.Emission = _RimColor.rgb * pow (rim, _RimPower); 
          o.Alpha = texC.a; 
          float3 add = o.Albedo * _AddColor.rgb;
          o.Albedo += add;
      } 
      ENDCG 
    }  
    Fallback "Diffuse" 
  } 
  
  
