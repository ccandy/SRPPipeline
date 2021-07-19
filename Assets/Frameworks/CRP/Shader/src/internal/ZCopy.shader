Shader "Hidden/CustomRenderPipeline/ZCopy"
{
     SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "CustomRenderPipeline"}
        LOD 100

        Pass
        {
            Name    "ZCopy"
            ZTest   Always
            ZWrite  On
            Cull    Off
            ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile _ _LINEAR_TO_SRGB_CONVERSION

            #include "../../include/BaseDefine/CommonDefine.hlsl"
            #include "../../include/BxDF/BxDFBaseFunction.hlsl"


            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

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

            TEXTURE2D_FLOAT(_SrcDepth);
            SAMPLER(sampler_SrcDepth);

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                //output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = UnityStereoTransformScreenSpaceTex(input.uv);
                //output.positionCS =  float4(input.uv.x*2.0 - 1.0, 1.0 - input.uv.y*2.0, 1.0, 1.0);
                output.positionCS = ComputeClipSpacePosition(input.uv, 0);
                return output;
            }

            //half4 Fragment(Varyings input) : SV_Target
            //{
            //    float4 col = EncodeFloatRGBA(SAMPLE_TEXTURE2D_DEF(_SrcDepth, input.uv).r);
    
            //    return col;//SAMPLE_TEXTURE2D_DEF(_SrcDepth, input.uv);
            //}

            //half4 Fragment(Varyings input, out float outDepth : SV_Depth) : SV_Target//SV_Depth
            //{    
            //    outDepth = SAMPLE_DEPTH_TEXTURE(_SrcDepth, sampler_SrcDepth, input.uv);
            //    return outDepth; 
            //}

            float Fragment(Varyings input) : SV_Depth
            {    
                return SAMPLE_DEPTH_TEXTURE(_SrcDepth, sampler_SrcDepth, input.uv);
            }
            ENDHLSL
        }
    }
}