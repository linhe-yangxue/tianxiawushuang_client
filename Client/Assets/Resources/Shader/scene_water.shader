
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.32;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,hqsc:True,hqlp:False,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.1280277,fgcg:0.1953466,fgcb:0.2352941,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32031,y:32826|diff-2-RGB,spec-14-RGB,gloss-273-OUT,emission-363-OUT,alpha-2-A;n:type:ShaderForge.SFN_Tex2d,id:2,x:32541,y:32794,ptlb:shui,ptin:_shui,ntxv:0,isnm:False|UVIN-39-OUT;n:type:ShaderForge.SFN_Tex2d,id:14,x:33003,y:33061,ptlb:node_2_copy,ptin:_node_2_copy,ntxv:0,isnm:False|UVIN-20-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:19,x:33194,y:32897,uv:0;n:type:ShaderForge.SFN_Panner,id:20,x:33239,y:33083,spu:0,spv:0.2;n:type:ShaderForge.SFN_Lerp,id:21,x:32801,y:32976|A-19-UVOUT,B-14-R,T-562-OUT;n:type:ShaderForge.SFN_Slider,id:22,x:32924,y:33277,ptlb:speed,ptin:_speed,min:0,cur:0,max:0.1;n:type:ShaderForge.SFN_Lerp,id:39,x:32598,y:33049|A-21-OUT,B-47-UVOUT,T-49-OUT;n:type:ShaderForge.SFN_TexCoord,id:47,x:32838,y:33112,uv:0;n:type:ShaderForge.SFN_Vector1,id:49,x:32819,y:33342,v1:8;n:type:ShaderForge.SFN_Vector1,id:273,x:32425,y:32932,v1:1;n:type:ShaderForge.SFN_Tex2d,id:352,x:33099,y:33381,ptlb:nodm,ptin:_nodm,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:363,x:32751,y:33433|A-352-RGB,B-376-OUT;n:type:ShaderForge.SFN_Slider,id:376,x:32974,y:33593,ptlb:node_376,ptin:_node_376,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Vector1,id:562,x:33374,y:33338,v1:0.01;proporder:2-14-22-352-376;pass:END;sub:END;*/

Shader "Scene/Water" {
    Properties {
        _shui ("shui", 2D) = "white" {}
        _node_2_copy ("node_2_copy", 2D) = "white" {}
        _speed ("speed", Range(0, 0.1)) = 0
        _nodm ("nodm", 2D) = "white" {}
        _node_376 ("node_376", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 5
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _TimeEditor;
            uniform sampler2D _shui; uniform float4 _shui_ST;
            uniform sampler2D _node_2_copy; uniform float4 _node_2_copy_ST;
            uniform sampler2D _nodm; uniform float4 _nodm_ST;
            uniform float _node_376;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 uv0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.uv0;
                o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
                float3 normalDirection =  i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;
////// Emissive:
                float2 node_575 = i.uv0;
                float3 emissive = (tex2D(_nodm,TRANSFORM_TEX(node_575.rg, _nodm)).rgb*_node_376);
///////// Gloss:
                float gloss = 1.0;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float4 node_576 = _Time + _TimeEditor;
                float2 node_20 = (node_575.rg+node_576.g*float2(0,0.2));
                float4 node_14 = tex2D(_node_2_copy,TRANSFORM_TEX(node_20, _node_2_copy));
                float3 specularColor = node_14.rgb;
                float3 specular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                float2 node_39 = lerp(lerp(i.uv0.rg,float2(node_14.r,node_14.r),0.01),i.uv0.rg,8.0);
                float4 node_2 = tex2D(_shui,TRANSFORM_TEX(node_39, _shui));
                finalColor += diffuseLight * node_2.rgb;
                finalColor += specular;
                finalColor += emissive;
/// Final Color:
                return fixed4(finalColor,node_2.a);
            }
            ENDCG
        }
        Pass {
            Name "ForwardAdd"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            Fog { Color (0,0,0,0) }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _TimeEditor;
            uniform sampler2D _shui; uniform float4 _shui_ST;
            uniform sampler2D _node_2_copy; uniform float4 _node_2_copy_ST;
            uniform sampler2D _nodm; uniform float4 _nodm_ST;
            uniform float _node_376;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 uv0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.uv0;
                o.normalDir = mul(float4(v.normal,0), _World2Object).xyz;
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
/////// Normals:
                float3 normalDirection =  i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 diffuse = max( 0.0, NdotL) * attenColor;
///////// Gloss:
                float gloss = 1.0;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                NdotL = max(0.0, NdotL);
                float4 node_578 = _Time + _TimeEditor;
                float2 node_577 = i.uv0;
                float2 node_20 = (node_577.rg+node_578.g*float2(0,0.2));
                float4 node_14 = tex2D(_node_2_copy,TRANSFORM_TEX(node_20, _node_2_copy));
                float3 specularColor = node_14.rgb;
                float3 specular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow) * specularColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                float2 node_39 = lerp(lerp(i.uv0.rg,float2(node_14.r,node_14.r),0.01),i.uv0.rg,8.0);
                float4 node_2 = tex2D(_shui,TRANSFORM_TEX(node_39, _shui));
                finalColor += diffuseLight * node_2.rgb;
                finalColor += specular;
/// Final Color:
                return fixed4(finalColor * node_2.a,0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
