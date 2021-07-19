Shader "CustomRenderPipeline/Unlit"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        [HDR]_BaseColor("Base Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Metallic("Metallic", Range(0.0,1.0)) = 0.0
        _Roughness("Roughness",  Range(0.0,1.0)) = 1.0
        _AO("AO", Range(0.0,1.0)) = 1.0

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0

        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "CustomPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
			Tags{"LightMode" = "CustomForward"}

            HLSLPROGRAM

            #pragma multi_compile_instancing

            #include "../../Include/Pass/Unlit/UnlitPass.hlsl"

            #pragma vertex UnlitVert
            #pragma fragment UnlitFrag

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

            HLSLPROGRAM

            #pragma multi_compile_instancing

            #include "../../Include/Pass/PBR/ShadowCasterPass.hlsl"

            #pragma vertex ShadowPassVert
            #pragma fragment ShadowPassFrag

            ENDHLSL
        }
    }

}