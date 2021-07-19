Shader "Hidden/CustomRenderPipeline/DeferredLighting"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "CustomRenderPipeline"}
        LOD 100

        Pass
        {
            Name    "DeferredMainDirectionalLightingWithGI"
            //Blend   One OneMinusSrcAlpha
            Blend   SrcAlpha OneMinusSrcAlpha
            ZTest   Always
            ZWrite  Off
            Cull    Off


            HLSLPROGRAM

            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment USE_ENV_CUBE /*USE_ENV_CUBE2D*/ USE_ENV_UNITY_CUBE
            //// Required to compile gles 2.0 with standard srp library
            //#pragma prefer_hlslcc gles
            //#pragma exclude_renderers d3d11_9x
            #pragma vertex DeferredLightingVertex
            #pragma fragment MainLightingFragment

            #include "../../Include/Pass/PBR/DeferredLightingPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name    "DeferredAdditionalDirectionalLighting"
            Blend   One      One
            ZTest   Always
            ZWrite  Off
            Cull    Off

            HLSLPROGRAM


            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma vertex DeferredLightingVertex
            #pragma fragment DirectionalLightingFragment

            #include "../../Include/Pass/PBR/DeferredLightingPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name    "DeferredAdditionalPointLighting"
            Blend   One      One
            ZTest   Always
            ZWrite  Off
            Cull    Off

            HLSLPROGRAM


            //#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma vertex DeferredLightingVertex
            #pragma fragment PointLightingFragment

            #include "../../Include/Pass/PBR/DeferredLightingPass.hlsl"

            ENDHLSL
        }
    }

    
}