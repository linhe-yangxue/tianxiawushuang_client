// ©2012 Hedgehog Team, do not sale. 

Shader "Active/RimTransparent" { 

    Properties { 
      _MainTex ("Base (RGBA)", 2D) = "white" {} 
      _RimColor ("Rim Color", Color) = (0,0,0,0.0) 
      _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0    	  
   	  _AddColor ("Add Color", Color) = (0,0,0,0)
    } 
    SubShader { 
      Tags { "RenderType" = "Transparent" } 
      
      Alphatest Greater 0
	  ZWrite On
	  ZTest LEqual
      
      Blend SrcAlpha OneMinusSrcAlpha 
      
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
             
      struct Input { 
          float2 uv_MainTex; 
          float3 viewDir; 
      }; 
      sampler2D _MainTex; 
      float4 _RimColor; 
      float _RimPower; 
      float4 _AddColor;
       
      void surf (Input IN, inout SurfaceOutput o) { 
      		float4 texC = tex2D (_MainTex, IN.uv_MainTex);
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


  
