Shader "CustomRenderPipeline/DeferredLitAnisotropy"
{
     Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        [HDR]_BaseColor("Base Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        [MaterialToggle(USE_HEIGHT_MAP)]HEIGHT_MAP("Is Use HeightMap", Int) = 0
        _HeightMap("Height Map", 2D) = "black" {}
        _Height("Height", float)    = 1.0

        [Space(15)][Header(Normal Map Properties)]
		[MaterialToggle(USE_NORMAL_MAP)]NORMALMAP("Is Use NormalMap", Int) = 1
        _NormalTex("Normal Texture", 2D) = "bump" {}

        _MetalRoughAOTex("R: Metal| G: Roughness| B: AO| A: Ansicos Texture", 2D) = "white" {}
        _Metallic("Metallic", Range(0.0,1.0)) = 0.0
        _Roughness("Roughness",  Range(0.0,1.0)) = 1.0
        _OcclusionStrength("AO", Range(0.0,1.0)) = 1.0
        _Anisotropy("Anisotropy Level", Range(0.0,1.0)) = 1.0

        [HideInInspector][HDR]_FuzzColorClothLv("FuzzColor|ClothLv", Color) = (1,1,1,1)

        _EmissionTex("EmissionTex Texture", 2D) = "black" {}

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
            Name "GBuffer"
			Tags{"LightMode" = "GBuffer"}

           ZTest Equal

            HLSLPROGRAM

            #pragma multi_compile_instancing
            
            #pragma shader_feature __ USE_HEIGHT_MAP
			#pragma shader_feature __ USE_NORMAL_MAP
			#pragma shader_feature __ USE_EMISSION_MAP

            #pragma vertex GBufferLitVert
            #pragma fragment GBufferLitAnisotropyFrag

            #include "../../Include/Pass/PBR/GBufferPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "EarlyZ"
			Tags{"LightMode" = "EarlyZ"}

            ZWrite On
			ZTest LEqual
			Cull[_Cull]
            //ColorMask 0

            HLSLPROGRAM

            #pragma multi_compile_instancing

            #pragma shader_feature _ALPHATEST_ON

            #include "../../Include/Pass/PBR/EarlyZPass.hlsl"

            #pragma vertex EarlyZPassVert
            #pragma fragment EarlyZPassFrag

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
			ZTest LEqual
			Cull[_Cull]
            ColorMask 0

            HLSLPROGRAM

            #pragma multi_compile_instancing

            #pragma shader_feature _ALPHATEST_ON

            #include "../../Include/Pass/PBR/ShadowCasterPass.hlsl"

            #pragma vertex ShadowPassVert
            #pragma fragment ShadowPassFrag

            ENDHLSL
        }
    }

}