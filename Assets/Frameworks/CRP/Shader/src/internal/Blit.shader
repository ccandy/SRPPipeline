Shader "Hidden/CustomRenderPipeline/Blit"
{
     SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "CustomRenderPipeline"}
        LOD 100

        Pass
        {
            Name    "Blit"
            ZTest   Always
            ZWrite  Off
            Cull    Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile _ _LINEAR_TO_SRGB_CONVERSION

            #include "../../include/BaseDefine/CommonDefine.hlsl"

            #ifdef _LINEAR_TO_SRGB_CONVERSION
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #endif

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                half4 positionCS    : SV_POSITION;
                half2 uv            : TEXCOORD0;
            };

            TEXTURE2D(_SourceTex);
            SAMPLER(sampler_SourceTex);

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = UnityStereoTransformScreenSpaceTex(input.uv);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv);
                #ifdef _LINEAR_TO_SRGB_CONVERSION
                col = LinearToSRGB(col);
                #endif
                return col;
            }
            ENDHLSL
        }
    }
}