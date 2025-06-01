Shader "Hidden/URP/Pixelation"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PixelSize ("Pixel Size", Float) = 200
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		Pass
		{
			Name "PixelationPass"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_TexelSize;
			float _PixelSize;

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			Varyings vert(Attributes input)
			{
				Varyings output;
				output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = input.uv;
				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				// デバッグ用：赤一色にする
				return half4(1, 0, 0, 1);
				// float2 uv = floor(input.uv * _PixelSize) / _PixelSize;
				// return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
			}
			ENDHLSL
		}
	}
	Fallback Off
}
