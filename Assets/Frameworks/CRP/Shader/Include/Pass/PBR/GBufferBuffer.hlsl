#ifndef GBUFFER_BUFFER_HLSL_
#define	GBUFFER_BUFFER_HLSL_

#include "../../BaseDefine/ConstDefine.hlsl"
#include "../../BaseDefine/CommonDefine.hlsl"
#include "../../BaseDefine/GBufferBase.hlsl"

CBUFFER_START(UnityPerMaterial)
real4	_BaseMap_ST;
real4	_BaseColor;
real	_Cutoff;

real    _Height;

real    _OcclusionStrength;
real	_Roughness;
real	_Metallic;
real	_Anisotropy;

real4   _EmissionColor;

real4   _FuzzColorClothLv;

real    _ClearCoat;

real    _ClearCoatRoughness;
CBUFFER_END

TEXTURE2D_DEF(_BaseMap);
TEXTURE2D_DEF(_NormalTex);
TEXTURE2D_DEF(_EmissionTex);
TEXTURE2D_DEF(_MetalRoughAOTex);

TEXTURE2D_DEF(_HeightMap);

#endif