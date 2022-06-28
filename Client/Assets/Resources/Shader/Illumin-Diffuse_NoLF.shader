Shader "Self-Illumin/Diffuse (NoLF)" {
		Properties{
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB),Illum(A)", 2D) = "white"{}
		_MainTex_Alpha("Illum (A)", 2D) = "white" {}
		_UseSecondAlpha("Is Use Second Alpha", int) = 0		
		_EmissionLM("Emission (Lightmapper)", Float) = 0
		_IlluminCtrl("Illumin Control", float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200
		Pass
		{
			Name "ForwardBase"
			Tags{ "LightMode" = "ForwardBase" }
			Fog{ Mode Off }

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#define UNITYCG_PASS_FORWARDBASE
#include "UnityCG.cginc"
#pragma multi_compile_fwdbase
#pragma exclude_renderers d3d11_9x
#pragma target 2.0

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform sampler2D _MainTex_Alpha;
			uniform int _UseSecondAlpha;
			uniform float4 _Color;
			uniform float _IlluminCtrl;

			fixed4 tex2D_ETC1(sampler2D sa, sampler2D sb, fixed2 v)
			{
				fixed4 col = tex2D(sa, v);
				fixed alp = tex2D(sb, v).r;
				col.a = min(col.a, alp);
				return col;
			}

			struct VertexInput
			{
				float4 vertex : POSITION;
				float4 uv0 : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float4 uv0 : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				o.uv0 = v.uv0;
				o.vertexColor = v.vertexColor;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			fixed4 frag(VertexOutput o) : COLOR
			{
				float4 col = tex2D_ETC1(_MainTex, _MainTex_Alpha, TRANSFORM_TEX(o.uv0.rg, _MainTex)) * _Color;
				col.rgb = o.vertexColor.rgb * col.rgb;
				return fixed4(col.rgb *(1 + col.a * _IlluminCtrl), 1);
			}
				ENDCG
		}

	}
	FallBack "Diffuse"
}
