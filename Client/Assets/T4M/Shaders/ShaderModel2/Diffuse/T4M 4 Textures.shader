Shader "T4MShaders/ShaderModel2/Diffuse/T4M 4 Textures" {
Properties {
	_Splat0 ("Layer 1", 2D) = "white" {}
	_Splat1 ("Layer 2", 2D) = "white" {}
	_Splat2 ("Layer 3", 2D) = "white" {}
	_Splat3 ("Layer 4", 2D) = "white" {}
	_Control ("Control (RGBA)", 2D) = "white" {}
	_Control_Alpha("Control Alpha",2D) = "white"{}
	//_MainTex ("Never Used", 2D) = "white" {}
	_ColorControl("ColorControl",float) = 0.5
}

	               
SubShader {
    Tags {
   "SplatCount" = "4"
   "RenderType" = "Opaque"
    }
CGPROGRAM
#pragma surface surf Lambert 
#pragma exclude_renderers xbox360 ps3

struct Input {
	
    float2 uv_Splat0 : TEXCOORD0;
    float2 uv_Splat1 : TEXCOORD1;
    float2 uv_Splat2 : TEXCOORD2;
    float2 uv_Splat3 : TEXCOORD3;
    float2 uv_Control : TEXCOORD4;
};

//2
sampler2D _Control_Alpha;
//3
fixed4 tex2D_ETC1(sampler2D sa,sampler2D sb,fixed2 v)        
{                                                           
    fixed4 col = tex2D(sa,v);                                  
    fixed alp = tex2D(sb,v).r;                                 
    col.a = min(col.a,alp) ;                                   
    return col;                                                

}   
fixed _ColorControl;
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
 
void surf (Input IN, inout SurfaceOutput o) {
    fixed4 splat_control = tex2D_ETC1 (_Control,_Control_Alpha,IN.uv_Control);
		
    fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
    fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
    fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
    fixed3 lay4 = tex2D (_Splat3, IN.uv_Splat3);
    o.Alpha = 1.0;
    o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a) * _ColorControl;

}
ENDCG 
}


    SubShader
    {

            CGPROGRAM
            #pragma surface surf Lambert 
            //#pragma exclude_renderers

            struct Input 
            {
                float2 uv_Splat0 : TEXCOORD0;
                float2 uv_Splat1 : TEXCOORD1;
                //float2 uv_Splat3 : TEXCOORD3;
                float2 uv_Control : TEXCOORD2;
            };

            //2
            //sampler2D _Control_Alpha;
            //3

            fixed _ColorControl;
            sampler2D _Control;
            sampler2D _Splat0,_Splat1,_Splat2;
 
            void surf (Input IN, inout SurfaceOutput o) 
            {
                fixed4 splat_control = tex2D (_Control,IN.uv_Control);
		
                fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
                fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
                o.Alpha = 1.0;
                o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g) * _ColorControl;
            }
            ENDCG 


            Blend One One
			
            CGPROGRAM
			
            #pragma surface surf Lambert 
            #pragma exclude_renderers

            struct Input 
            {
                //float2 uv_Splat0 : TEXCOORD0;
                //float2 uv_Splat1 : TEXCOORD1;
                float2 uv_Splat2 : TEXCOORD0;
                float2 uv_Splat3 : TEXCOORD1;
                float2 uv_Control : TEXCOORD2;
            };

            //2
            sampler2D _Control_Alpha;
            //3
            fixed4 tex2D_ETC1(sampler2D sa,sampler2D sb,fixed2 v)        
            {                                                           
                fixed4 col = tex2D(sa,v);                                  
                fixed alp = tex2D(sb,v).r;                                 
                col.a = min(col.a,alp) ;                                   
                return col;                                                

            }   
            fixed _ColorControl;
            sampler2D _Control;
            sampler2D _Splat2;
            sampler2D _Splat3;
 
            void surf (Input IN, inout SurfaceOutput o) 
            {
                fixed4 splat_control = tex2D_ETC1 (_Control,_Control_Alpha,IN.uv_Control);
                fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
                fixed3 lay4 = tex2D (_Splat3, IN.uv_Splat3);
                o.Alpha = 1.0;
                o.Albedo.rgb =  ( lay3 * splat_control.b + lay4 * splat_control.a) * _ColorControl ;
                //o.Albedo.rgb =  1.0f * 0.5f  * 0.2f ;
                //o.Albedo.rgb = fixed3(1,1,1);
            }

            ENDCG
		
    }
}
