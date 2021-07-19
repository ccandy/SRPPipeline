Shader "Hidden/CombineTexture"
{
	Properties
	{
		_RTex("R:", 2D) = "white"{}
		
		_GTex("G:", 2D) = "white"{}

		_BTex("B:", 2D) = "white"{}

		_ATex("A:", 2D) = "white"{}

		_RTex_DotMask("_RTex_DotMask", Vector) = (0.3,0.59,0.11,0)
		_GTex_DotMask("_GTex_DotMask", Vector) = (0.3,0.59,0.11,0)
		_BTex_DotMask("_BTex_DotMask", Vector) = (0.3,0.59,0.11,0)
		_ATex_DotMask("_ATex_DotMask", Vector) = (0.3,0.59,0.11,0)

		_IsRLinearToGamma("IsRLinearToGamma", Float) = 0

		_IsGLinearToGamma("IsGLinearToGamma", Float) = 0

		_IsBLinearToGamma("IsBLinearToGamma", Float) = 0

		_IsALinearToGamma("IsALinearToGamma", Float) = 0
	}

	SubShader
	{
		ZTest Off
		Cull Off
		ZWrite Off
		Blend Off
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform float _LUTCurveScale;

			uniform float _IsRLinearToGamma;
			uniform float _IsGLinearToGamma;
			uniform float _IsBLinearToGamma;
			uniform float _IsALinearToGamma;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;

				o.uv = v.texcoord;// fmod(v.texcoord.xy, 1);
				o.pos = float4(o.uv * 2 - 1, 0, 1);
				o.pos.y = -o.pos.y;

				return o;
			}

			float4 _RTex_DotMask;
			float4 _GTex_DotMask;
			float4 _BTex_DotMask;
			float4 _ATex_DotMask;

			sampler2D _RTex;
			sampler2D _GTex;
			sampler2D _BTex;
			sampler2D _ATex;

			float4 frag(v2f i) : SV_Target
			{
				float R = dot(tex2D(_RTex, i.uv), _RTex_DotMask);
				float G = dot(tex2D(_GTex, i.uv), _GTex_DotMask);
				float B = dot(tex2D(_BTex, i.uv), _BTex_DotMask);
				float A = dot(tex2D(_ATex, i.uv), _ATex_DotMask);

				R = _IsRLinearToGamma > 0 ? LinearToGammaSpaceExact(R) : R;
				G = _IsGLinearToGamma > 0 ? LinearToGammaSpaceExact(G) : G;
				B = _IsBLinearToGamma > 0 ? LinearToGammaSpaceExact(B) : B;
				A = _IsALinearToGamma > 0 ? LinearToGammaSpaceExact(A) : A;

				float4 col = float4(R,G,B,A);

#ifdef UNITY_COLORSPACE_GAMMA
				return col;
#else
				col.xyz = GammaToLinearSpace(col.xyz);
				return col;
#endif
			}

			ENDCG
		}

	}
	FallBack Off
}

