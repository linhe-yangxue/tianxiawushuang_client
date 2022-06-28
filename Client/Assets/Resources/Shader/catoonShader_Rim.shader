//Shader "GDE/JLCB/catoonShader_Rim" {
//       Properties {
//        _MainTex ("MainTex", 2D) = "white" {}
//        _MainTex_Alpha("MainTex Alpha",2D) ="white"{}
//        _MainColor ("Main Color", Color) = (0,0,0,1)
//        //_specular ("specular", 2D) = "black" {}
//        _RimColor ("Rim Color",Color) =  (0.5652891,0.4477185,0.6691177,1)
//        _RimMag("Rim Magnitude",range(0,1)) = 0
//        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
//    }
//    Subshader
//    {
//        Pass {
//            Name "ForwardBase"
//            Tags 
//            {
//                "LightMode"="ForwardBase"
//            }
//            Blend SrcAlpha OneMinusSrcAlpha
//            Cull Back

//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma multi_compile_fwdbase

//            #define UNITY_PASS_FORWARDBASE
//            #include "UnityCG.cginc"
//            #include "AutoLight.cginc"

//            #pragma exclude_renderers  
//            #pragma target 2.0
//            uniform float4 _LightColor0;
//            uniform sampler2D _MainTex;
//            uniform sampler2D _MainTex_Alpha;
//            uniform float4 _MainTex_ST;
//            uniform float4 _MainColor;
//            float4 _RimColor;
//            uniform float _RimMag;
//            uniform float _Cutoff;

//            struct VertexInput {
//                float4 vertex : POSITION;
//                float3 normal : NORMAL;
//                float4 uv0 : TEXCOORD0;
//                float4 color : COLOR;
//            };
//            struct VertexOutput {
//                float4 pos : SV_POSITION;
//                float4 uv0 : TEXCOORD0;
//                float4 posWorld : TEXCOORD1;
//                float3 normalDir : TEXCOORD2;
//                float4 color : COLOR;
//                LIGHTING_COORDS(3,4)
//            };

//            VertexOutput vert (VertexInput v) 
//            {
//                VertexOutput o;
//                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//                o.uv0 = v.uv0;
//                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
//                float dotProject = 1 - dot(v.normal,viewDir);
               
//                float3 normalDirection = normalize(mul(float4(v.normal,0),_World2Object).xyz);
//                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
//                float atten = 1.0;
//                float3 diffuseReflection = atten * _LightColor0.xyz * max(0,dot(normalDirection,lightDirection));
//                float3 lightFinal = diffuseReflection +UNITY_LIGHTMODEL_AMBIENT.xyz;
//                o.color = smoothstep(1-_RimMag,1.0,dotProject) * _RimColor + float4(lightFinal,1.0);  

//                TRANSFER_VERTEX_TO_FRAGMENT(o)
//                return o;
//            }

//            int _UseSecondAlpha;
//            fixed4 tex2D_ETC1(sampler2D sa,sampler2D sb,fixed2 v)        
//            {                                                           
//                fixed4 col = tex2D(sa,v);                                  
//                fixed alp = tex2D(sb,v).r;                                 
//                col.a = min(col.a,alp) ;                                   
//                return col;                                                
//            }   

//            fixed4 frag(VertexOutput i) : COLOR 
//            {
//                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
//                float3 normalDirections =  i.normalDir;
//                float3 lightDirections = normalize(_WorldSpaceLightPos0.xyz);
//                float3 halfDirection = normalize(viewDirection+lightDirections);
//                float attenuation = LIGHT_ATTENUATION(i);
//                float3 attenColor = attenuation * _LightColor0.xyz;
//                float NdotL = dot( normalDirections, lightDirections );
//                float4 node_546 = tex2D_ETC1(_MainTex,_MainTex_Alpha,TRANSFORM_TEX(i.uv0.rg, _MainTex));
//                float3 w = node_546.rgb*0.5; 
//                float3 NdotLWrap = NdotL * ( 1.0 - w );
//                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
//                float3 diffuse = forwardLight * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
//                float3 emissive = _MainColor.rgb;
//                float gloss = 0.5;
//                float specPow = exp2( gloss * 10.0+1.0);
//                NdotL = max(0.0, NdotL);          
//                clip(node_546.a - _Cutoff);
//                float3 finalColor =  node_546.rgb *_LightColor0;             //贴图原色
//                finalColor += emissive;                                      //自发光
//                finalColor += i.color;                                      //边缘光
                
                
//                return fixed4(finalColor,node_546.a);
//            }
//            ENDCG
//        }
//    }
//    Fallback "Diffuse"
//}

Shader "GDE/JLCB/catoonShader_Rim" 
{
     Properties 
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _MainTex_Alpha("MainTex Alpha",2D) = "white"{}
        _buff_color ("buff_color", Color) = (0,0,0,1)
		_rimColor ("Rim Color",Color) =  (0.5652891,0.4477185,0.6691177,1)
        _rimColorCtr("Rim Control Color",float) = 0
		_rimMag("Rim Magnitude",range(0,2)) = 1.8
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers  
            //#pragma target 2.0
            uniform float4 _LightColor0;
            uniform sampler2D _MainTex; uniform sampler2D _MainTex_Alpha;uniform float4 _MainTex_ST;
            uniform float4 _buff_color;
			uniform float4 _rimColor;
            uniform float _rimColorCtr;
			uniform float _rimMag;
            uniform float _Cutoff;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 uv0 : TEXCOORD0;
                float4 color : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 rimColor :TEXCOORD3;
                float4 color : COLOR;
                LIGHTING_COORDS(4,5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.uv0;
                o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
                o.posWorld = mul(_Object2World, v.vertex);
                
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                float dotProject = 1 -dot(v.normal,viewDir);

                float3 normalDirection = normalize(mul(float4(v.normal,0),_World2Object).xyz);
                float3 lightDirection = normalize(WorldSpaceLightDir(v.vertex).xyz);
                float atten = 2.0;
                float3 diffuseReflection = 2*atten * _LightColor0.xyz * max(0,dot(normalDirection,lightDirection));
                float3 lightFinal = diffuseReflection;

                o.color =  float4(lightFinal,1) ;
                o.rimColor = smoothstep(1-_rimMag,0.7*_rimColorCtr,dotProject) *  _rimColor ;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            int _UseSecondAlpha;
            fixed4 tex2D_ETC1(sampler2D sa,sampler2D sb,fixed2 v)        
            {                                                           
                fixed4 col = tex2D(sa,v);                                  
                fixed alp = tex2D(sb,v).r;                                 
                col.a = min(col.a,alp) ;                                   
                return col;                                                
            }   


            fixed4 frag(VertexOutput i) : COLOR {
                float attenuation = LIGHT_ATTENUATION(i);
                float4 mainTex = tex2D_ETC1(_MainTex,_MainTex_Alpha,TRANSFORM_TEX(i.uv0.rg, _MainTex));
                clip(mainTex.a - _Cutoff);
                //float gloss = 0.5;
                float3 finalColor = 0;   
                finalColor += mainTex.rgb * i.color + i.rimColor.xyz;       //主贴图
                finalColor += _buff_color;                         //自发光
                return fixed4(finalColor,1);
//return i.color;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}