using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


namespace Frameworks.CRP
{
	public struct CameraData
	{
		public Matrix4x4										viewMatrix;
		public Matrix4x4										projMatrix;

		public RenderTexture									targetTexture;
		public RenderTextureDescriptor							cameraTargetDescriptor;

		public Camera											camera;
		public bool												isOverlay;
		public bool												isClearDepth;
		public CameraType										cameraType;
		public SortingCriteria									defaultOpaqueSortFlags;

		public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions;

		public Color											clearColor;

		public Rect												pixelRect;
		public int												pixelWidth;
		public int												pixelHeight;
		public float											aspectRatio;
		public float											renderScale;
		public bool												clearDepth;

		public bool												isDefaultViewport;
		public bool												isHdrEnabled;
		public bool												requiresDepthTexture;
		public bool												requiresOpaqueTexture;

		public int												renderPassBlockIndex;
#if ENABLE_VR && ENABLE_XR_MODULE
        public bool xrRendering;
#endif
		internal bool requireSrgbConversion
		{
			get
			{
#if ENABLE_VR && ENABLE_XR_MODULE
                if (xr.enabled)
                    return !xr.renderTargetDesc.sRGB && (QualitySettings.activeColorSpace == ColorSpace.Linear);
#endif

				return Display.main.requiresSrgbBlitToBackbuffer;
			}
		}

		public Matrix4x4 GetGPUProjectionMatrix()
		{
			return GL.GetGPUProjectionMatrix( projMatrix, IsCameraProjectionMatrixFlipped());
		}

		public bool IsCameraProjectionMatrixFlipped()
		{
//			bool renderingToBackBufferTarget = cameraColorTarget == BuiltinRenderTextureType.CameraTarget;
//#if ENABLE_VR && ENABLE_XR_MODULE
//            if (xr.enabled)
//                renderingToBackBufferTarget |= renderer.cameraColorTarget == xr.renderTarget && !xr.renderTargetIsRenderTexture;
//#endif
			bool renderingToTexture = targetTexture != null;
			return SystemInfo.graphicsUVStartsAtTop && renderingToTexture;

		}

		public static CameraData NewCameraData(Camera camera, PipelineAdditionalCameraData pipelineAdditional)
		{
			var asset = CustomRenderPipeline.bindAsset;

			CameraData cameraData = new CameraData();

			cameraData.camera		= camera;

			cameraData.viewMatrix	= camera.worldToCameraMatrix;
			cameraData.projMatrix	= camera.projectionMatrix;

			cameraData.targetTexture = camera.targetTexture;

			cameraData.cameraType = camera.cameraType;

			cameraData.isClearDepth = !(!pipelineAdditional.IsOverlayClearDepth && pipelineAdditional.IsOverlayCamera);

			cameraData.isOverlay	= pipelineAdditional.IsOverlayCamera;

			cameraData.clearColor = CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor);

			if (cameraData.cameraType == CameraType.SceneView)
			{
				//cameraData.volumeLayerMask = 1; // "Default"
				//cameraData.volumeTrigger = null;
				//cameraData.isStopNaNEnabled = false;
				//cameraData.isDitheringEnabled = false;
				//cameraData.antialiasing = AntialiasingMode.None;
				//cameraData.antialiasingQuality = AntialiasingQuality.High;
#if ENABLE_VR && ENABLE_XR_MODULE
                cameraData.xrRendering = false;
#endif
				cameraData.renderPassBlockIndex = 0;
			}
			else if (pipelineAdditional != null)
			{
				//cameraData.volumeLayerMask = baseAdditionalCameraData.volumeLayerMask;
				//cameraData.volumeTrigger = baseAdditionalCameraData.volumeTrigger == null ? baseCamera.transform : baseAdditionalCameraData.volumeTrigger;
				//cameraData.isStopNaNEnabled = baseAdditionalCameraData.stopNaN && SystemInfo.graphicsShaderLevel >= 35;
				//cameraData.isDitheringEnabled = baseAdditionalCameraData.dithering;
				//cameraData.antialiasing = baseAdditionalCameraData.antialiasing;
				//cameraData.antialiasingQuality = baseAdditionalCameraData.antialiasingQuality;
#if ENABLE_VR && ENABLE_XR_MODULE
                cameraData.xrRendering = pipelineAdditional.allowXRRendering && m_XRSystem.RefreshXrSdk();
#endif
				cameraData.renderPassBlockIndex = pipelineAdditional.RenderPassBlockIndex;
			}
			else
			{
				//cameraData.volumeLayerMask = 1; // "Default"
				//cameraData.volumeTrigger = null;
				//cameraData.isStopNaNEnabled = false;
				//cameraData.isDitheringEnabled = false;
				//cameraData.antialiasing = AntialiasingMode.None;
				//cameraData.antialiasingQuality = AntialiasingQuality.High;
#if ENABLE_VR && ENABLE_XR_MODULE
                cameraData.xrRendering = m_XRSystem.RefreshXrSdk();
#endif
				cameraData.renderPassBlockIndex = 0;
			}

			///////////////////////////////////////////////////////////////////
			// Settings that control output of the camera                     /
			///////////////////////////////////////////////////////////////////


			int msaaSamples = 1;
			if (camera.allowMSAA && (int)asset.MSAASampleCount > 1)
				msaaSamples = (camera.targetTexture != null) ? camera.targetTexture.antiAliasing : (int)asset.MSAASampleCount;
#if ENABLE_VR && ENABLE_XR_MODULE
            // Use XR's MSAA if camera is XR camera. XR MSAA needs special handle here because it is not per Camera.
            // Multiple cameras could render into the same XR display and they should share the same MSAA level.
            if (cameraData.xrRendering)
                msaaSamples = XRSystem.GetMSAALevel();
#endif

			cameraData.isHdrEnabled = camera.allowHDR && asset.SupportsHDR;

			Rect cameraRect = camera.rect;
			cameraData.pixelRect	= camera.pixelRect;
			cameraData.pixelWidth	= camera.pixelWidth;
			cameraData.pixelHeight	= camera.pixelHeight;
			cameraData.aspectRatio	= (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
			cameraData.isDefaultViewport = (!(Math.Abs(cameraRect.x) > 0.0f || Math.Abs(cameraRect.y) > 0.0f ||
				Math.Abs(cameraRect.width) < 1.0f || Math.Abs(cameraRect.height) < 1.0f));

			// Discard variations lesser than kRenderScaleThreshold.
			// Scale is only enabled for gameview.
			const float kRenderScaleThreshold = 0.05f;
			cameraData.renderScale = (Mathf.Abs(1.0f - asset.RenderScale) < kRenderScaleThreshold) ? 1.0f : asset.RenderScale;

#if ENABLE_VR && ENABLE_XR_MODULE
            //cameraData.xr = m_XRSystem.emptyPass;
            XRSystem.UpdateRenderScale(cameraData.renderScale);
#else
			//cameraData.xr = XRPass.emptyPass;
#endif

			var commonOpaqueFlags = SortingCriteria.CommonOpaque;
			var noFrontToBackOpaqueFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
			bool hasHSRGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
			bool canSkipFrontToBackSorting = (camera.opaqueSortMode == OpaqueSortMode.Default && hasHSRGPU) || camera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;

			cameraData.defaultOpaqueSortFlags = canSkipFrontToBackSorting ? noFrontToBackOpaqueFlags : commonOpaqueFlags;
			cameraData.captureActions = CameraCaptureBridge.GetCaptureActions(camera);

			bool needsAlphaChannel = Graphics.preserveFramebufferAlpha;
			cameraData.cameraTargetDescriptor = CustomRenderPipeline.CreateRenderTextureDescriptor(camera, cameraData.renderScale,
				cameraData.isHdrEnabled, msaaSamples, needsAlphaChannel);

			return cameraData;
		}
	}

	public struct LightData
	{
		public int mainLightIndex;
		public int TotalLightCount;
		public static VisibleLight[] Lights = new VisibleLight[256];

		public int DirectinalLightCount;
		public static int[] DirectinalLightIndices = new int[256];

		public int PointLightCount;
		public static int[] PointLightIndices = new int[256];

		public int SpotLightCount;
		public static int[] SpotLightIndices = new int[256];

		public void SetupLights(ref CullingResults cullingResults)
		{
			TotalLightCount = cullingResults.visibleLights.Length;
			DirectinalLightCount = 0;
			PointLightCount = 0;
			SpotLightCount = 0;

			if (TotalLightCount == 0)
			{
				mainLightIndex = - 1;
			}

			for (int i = 0; i < TotalLightCount && i < Lights.Length; ++i)
			{
				Lights[i] = cullingResults.visibleLights[i];
			}

			mainLightIndex = GetMainLightIndex(ref cullingResults);
			for (int i = 0; i < Lights.Length; ++i)
			{
				var light = Lights[i];
				switch (light.lightType)
				{
					case LightType.Directional:
						{
							if (i != mainLightIndex && DirectinalLightCount < DirectinalLightIndices.Length)
							{
								DirectinalLightIndices[DirectinalLightCount] = i;
								++DirectinalLightCount;
							}
						}
						break;
					case LightType.Point:
						{
							if (i != mainLightIndex && PointLightCount < PointLightIndices.Length)
							{
								PointLightIndices[PointLightCount] = i;
								++PointLightCount;
							}
						}
						break;
					case LightType.Spot:
						{
							if (i != mainLightIndex && SpotLightCount < SpotLightIndices.Length)
							{
								SpotLightIndices[SpotLightCount] = i;
								++SpotLightCount;
							}
						}
						break;
				}
			}
		}

		public static int GetMainLightIndex(ref CullingResults cullingResults)
		{
			int totalVisibleLights = cullingResults.visibleLights.Length;

			if (totalVisibleLights == 0)
				return -1;

			Light sunLight = RenderSettings.sun;
			int brightestDirectionalLightIndex = -1;
			float brightestLightIntensity = 0.0f;
			for (int i = 0; i < totalVisibleLights; ++i)
			{
				VisibleLight currVisibleLight = cullingResults.visibleLights[i];
				Light currLight = currVisibleLight.light;

				// Particle system lights have the light property as null. We sort lights so all particles lights
				// come last. Therefore, if first light is particle light then all lights are particle lights.
				// In this case we either have no main light or already found it.
				if (currLight == null)
					break;

				if (currVisibleLight.lightType != LightType.Directional)
				{
					continue;
				}

				// Sun source needs be a directional light
				if (currLight == sunLight)
					return i;

				// In case no sun light is present we will return the brightest directional light
				if (currLight.intensity > brightestLightIntensity)
				{
					brightestLightIntensity = currLight.intensity;
					brightestDirectionalLightIndex = i;
				}
			}

			return brightestDirectionalLightIndex;
		}

	}

	public struct ShadowSliceData
	{
		public Matrix4x4 viewMatrix;
		public Matrix4x4 projectionMatrix;
		public Matrix4x4 shadowTransform;
		public int offsetX;
		public int offsetY;
		public int resolution;
	}

	public struct ShadowData
	{
		public bool isUseShadowmap;
		public bool isUseSoftShadowMap;
		public int	shadowmapResolution;
		public int	tileShadowResolution;
		public int	shadowCascadesCount;
		public Vector4 shadowCascadesSplit;
		//public List<Vector4> bias;
	}

	public struct PostProcessingData
	{
		public bool isUseTonemapping;
		public bool isUseBloom;
	}

	public struct RenderingData
	{
		public CullingResults cullResults;
		public CameraData cameraData;
		public LightData lightData;
		public ShadowData shadowData;
		public PostProcessingData postProcessingData;
		public bool supportsDynamicBatching;
		public PerObjectData perObjectData;

		/// <summary>
		/// True if post-processing effect is enabled while rendering the camera stack.
		/// </summary>
		public bool postProcessingEnabled;

		public static readonly int ColorAttenmentID = Shader.PropertyToID("_ColorCameraTarget");
		public static readonly int DepthAttenmentID = Shader.PropertyToID("_DepthCameraTarget");

		public RenderTargetIdentifier orignCameraColorIdentifier;
		public RenderTargetIdentifier orignCameraDepthIdentifier;

		public RenderTargetIdentifier curCameraColorIdentifier;
		public RenderTargetIdentifier curCameraDepthIdentifier;

		public static RenderingData NewRenderingData(ScriptableRenderContext context, Camera camera, PipelineAdditionalCameraData additionalCameraData)
		{
			var asset = CustomRenderPipeline.bindAsset;

			RenderingData renderingData = new RenderingData();

			renderingData.cameraData = CameraData.NewCameraData(camera, additionalCameraData);

			ref CameraData cameraData = ref renderingData.cameraData;

			renderingData.orignCameraColorIdentifier = cameraData.targetTexture == null ? BuiltinRenderTextureType.CameraTarget : new RenderTargetIdentifier( cameraData.targetTexture, 0, CubemapFace.Unknown, -1);

			renderingData.orignCameraDepthIdentifier = cameraData.targetTexture == null ? BuiltinRenderTextureType.CameraTarget : new RenderTargetIdentifier( cameraData.targetTexture, 0, CubemapFace.Unknown, -1);

			renderingData.curCameraColorIdentifier = renderingData.orignCameraColorIdentifier;

			renderingData.curCameraDepthIdentifier = renderingData.orignCameraDepthIdentifier;

			ScriptableCullingParameters cullingParameters;

//#if ENABLE_VR && ENABLE_XR_MODULE
//            if (cameraData.xr.enabled)
//            {
//                cullingParams = cameraData.xr.cullingParams;

//                // Sync the FOV on the camera to match the projection from the XR device
//                if (!cameraData.camera.usePhysicalProperties)
//                    cameraData.camera.fieldOfView = Mathf.Rad2Deg * Mathf.Atan(1.0f / cullingParams.stereoProjectionMatrix.m11) * 2.0f;

//                return true;
//            }
//#endif

			camera.TryGetCullingParameters( camera.stereoEnabled, out cullingParameters);

			//bool isShadowCastingDisabled = !CustomRenderPipeline.bindAsset.supportsMainLightShadows && !CustomRenderPipeline.bindAsset.supportsAdditionalLightShadows;
			bool isShadowCastingDisabled = asset.shadowType == ShadowType.Disable;
			bool isShadowDistanceZero = Mathf.Approximately(asset.ShadowDistance, 0.0f);

			cullingParameters.shadowDistance = asset.ShadowDistance;

			if (isShadowCastingDisabled || isShadowDistanceZero)
			{
				cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
			}

			renderingData.shadowData.isUseShadowmap = !isShadowCastingDisabled;

			renderingData.shadowData.isUseSoftShadowMap = asset.shadowType == ShadowType.SoftShadow;

			renderingData.shadowData.shadowmapResolution = asset.ShadowMapResolution;

			renderingData.shadowData.shadowCascadesCount = asset.ShadowCascadeCount;
			renderingData.shadowData.shadowCascadesSplit = asset.CascadeSplits;

			renderingData.cullResults = context.Cull( ref cullingParameters);

			renderingData.lightData.SetupLights(ref renderingData.cullResults);

			renderingData.supportsDynamicBatching = asset.SupportsDynamicBatching;

			renderingData.perObjectData = PerObjectData.ReflectionProbes | PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.LightData | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask | PerObjectData.LightIndices;

			// TODO: Add later
			renderingData.postProcessingEnabled = false;

			return renderingData;
		}
	}
	public static class ShaderPropertyID
	{
		public static readonly int glossyEnvironmentColor			= Shader.PropertyToID("_GlossyEnvironmentColor");
		public static readonly int subtractiveShadowColor			= Shader.PropertyToID("_SubtractiveShadowColor");

		public static readonly int ambientSkyColor					= Shader.PropertyToID("unity_AmbientSky");
		public static readonly int ambientEquatorColor				= Shader.PropertyToID("unity_AmbientEquator");
		public static readonly int ambientGroundColor				= Shader.PropertyToID("unity_AmbientGround");

		public static readonly int time								= Shader.PropertyToID("_Time");
		public static readonly int sinTime							= Shader.PropertyToID("_SinTime");
		public static readonly int cosTime							= Shader.PropertyToID("_CosTime");
		public static readonly int deltaTime						= Shader.PropertyToID("unity_DeltaTime");
		public static readonly int timeParameters					= Shader.PropertyToID("_TimeParameters");

		public static readonly int scaledScreenParams				= Shader.PropertyToID("_ScaledScreenParams");
		public static readonly int worldSpaceCameraPos				= Shader.PropertyToID("_WorldSpaceCameraPos");
		public static readonly int screenParams						= Shader.PropertyToID("_ScreenParams");
		public static readonly int projectionParams					= Shader.PropertyToID("_ProjectionParams");
		public static readonly int zBufferParams					= Shader.PropertyToID("_ZBufferParams");
		public static readonly int orthoParams						= Shader.PropertyToID("unity_OrthoParams");

		public static readonly int viewMatrix						= Shader.PropertyToID("unity_MatrixV");
		public static readonly int projectionMatrix					= Shader.PropertyToID("glstate_matrix_projection");
		public static readonly int viewAndProjectionMatrix			= Shader.PropertyToID("unity_MatrixVP");

		public static readonly int inverseViewMatrix				= Shader.PropertyToID("unity_MatrixInvV");
		public static readonly int inverseProjectionMatrix			= Shader.PropertyToID("unity_MatrixInvP");
		public static readonly int inverseViewAndProjectionMatrix	= Shader.PropertyToID("unity_MatrixInvVP");

		public static readonly int cameraProjectionMatrix			= Shader.PropertyToID("unity_CameraProjection");
		public static readonly int inverseCameraProjectionMatrix	= Shader.PropertyToID("unity_CameraInvProjection");
		public static readonly int worldToCameraMatrix				= Shader.PropertyToID("unity_WorldToCamera");
		public static readonly int cameraToWorldMatrix				= Shader.PropertyToID("unity_CameraToWorld");

		public static readonly int sourceTex						= Shader.PropertyToID("_SourceTex");
		public static readonly int scaleBias						= Shader.PropertyToID("_ScaleBias");
		public static readonly int scaleBiasRt						= Shader.PropertyToID("_ScaleBiasRt");

		public static readonly int MainLightDirection				= Shader.PropertyToID("_MainLightDirection");
		public static readonly int MainLightColor					= Shader.PropertyToID("_MainLightColor");

		public static readonly int EnvCubeMap						= Shader.PropertyToID("_EnvCubeMap");
		public static readonly int MaxLod							= Shader.PropertyToID("_MaxLod");
		public static readonly int EnvironmentExposure				= Shader.PropertyToID("_EnvironmentExposure");
		public static readonly int SHAr								= Shader.PropertyToID("_SHAr");
		public static readonly int SHAg								= Shader.PropertyToID("_SHAg");
		public static readonly int SHAb								= Shader.PropertyToID("_SHAb");
		public static readonly int SHBr								= Shader.PropertyToID("_SHBr");
		public static readonly int SHBg								= Shader.PropertyToID("_SHBg");
		public static readonly int SHBb								= Shader.PropertyToID("_SHBb");
		public static readonly int SHC								= Shader.PropertyToID("_SHC");

		public static readonly int PlanarReflectionTexture			= Shader.PropertyToID("_PlanarReflectionTexture");
		// Required for 2D Unlit Shadergraph master node as it doesn't currently support hidden properties.
		//public static readonly int rendererColor					= Shader.PropertyToID("_RendererColor");
	}

	public static class ShaderKeywordStrings
	{
		public static readonly string MainLightShadows			= "_MAIN_LIGHT_SHADOWS";
		public static readonly string MainLightShadowCascades	= "_MAIN_LIGHT_SHADOWS_CASCADE";
		public static readonly string AdditionalLightsVertex	= "_ADDITIONAL_LIGHTS_VERTEX";
		public static readonly string AdditionalLightsPixel		= "_ADDITIONAL_LIGHTS";
		public static readonly string AdditionalLightShadows	= "_ADDITIONAL_LIGHT_SHADOWS";
		public static readonly string SoftShadows				= "_SHADOWS_SOFT";
		public static readonly string MixedLightingSubtractive	= "_MIXED_LIGHTING_SUBTRACTIVE"; // Backward compatibility
		public static readonly string LightmapShadowMixing		= "LIGHTMAP_SHADOW_MIXING";
		public static readonly string ShadowsShadowMask			= "SHADOWS_SHADOWMASK";

		public static readonly string DepthNoMsaa				= "_DEPTH_NO_MSAA";
		public static readonly string DepthMsaa2				= "_DEPTH_MSAA_2";
		public static readonly string DepthMsaa4				= "_DEPTH_MSAA_4";
		public static readonly string DepthMsaa8				= "_DEPTH_MSAA_8";

		public static readonly string LinearToSRGBConversion	= "_LINEAR_TO_SRGB_CONVERSION";

		public static readonly string SmaaLow					= "_SMAA_PRESET_LOW";
		public static readonly string SmaaMedium				= "_SMAA_PRESET_MEDIUM";
		public static readonly string SmaaHigh					= "_SMAA_PRESET_HIGH";
		public static readonly string PaniniGeneric				= "_GENERIC";
		public static readonly string PaniniUnitDistance		= "_UNIT_DISTANCE";
		public static readonly string BloomLQ					= "_BLOOM_LQ";
		public static readonly string BloomHQ					= "_BLOOM_HQ";
		public static readonly string BloomLQDirt				= "_BLOOM_LQ_DIRT";
		public static readonly string BloomHQDirt				= "_BLOOM_HQ_DIRT";
		public static readonly string UseRGBM					= "_USE_RGBM";
		public static readonly string Distortion				= "_DISTORTION";
		public static readonly string ChromaticAberration		= "_CHROMATIC_ABERRATION";
		public static readonly string HDRGrading				= "_HDR_GRADING";
		public static readonly string TonemapACES				= "_TONEMAP_ACES";
		public static readonly string TonemapNeutral			= "_TONEMAP_NEUTRAL";
		public static readonly string FilmGrain					= "_FILM_GRAIN";
		public static readonly string Fxaa						= "_FXAA";
		public static readonly string Dithering					= "_DITHERING";
		public static readonly string ScreenSpaceOcclusion		= "_SCREEN_SPACE_OCCLUSION";

		public static readonly string HighQualitySampling		= "_HIGH_QUALITY_SAMPLING";

		public static readonly string DOWNSAMPLING_SIZE_2		= "DOWNSAMPLING_SIZE_2";
		public static readonly string DOWNSAMPLING_SIZE_4		= "DOWNSAMPLING_SIZE_4";
		public static readonly string DOWNSAMPLING_SIZE_8		= "DOWNSAMPLING_SIZE_8";
		public static readonly string DOWNSAMPLING_SIZE_16		= "DOWNSAMPLING_SIZE_16";
		public static readonly string _SPOT						= "_SPOT";
		public static readonly string _DIRECTIONAL				= "_DIRECTIONAL";
		public static readonly string _POINT					= "_POINT";
		public static readonly string _ALPHATEST_ON				= "_ALPHATEST_ON";

		public static readonly string _USE_ENV_CUBE				= "USE_ENV_CUBE";
		//public static readonly string _DEFERRED_ADDITIONAL_LIGHT_SHADOWS = "_DEFERRED_ADDITIONAL_LIGHT_SHADOWS";
		//public static readonly string _GBUFFER_NORMALS_OCT		= "_GBUFFER_NORMALS_OCT";
		//public static readonly string _DEFERRED_SUBTRACTIVE_LIGHTING = "_DEFERRED_SUBTRACTIVE_LIGHTING";
		//public static readonly string LIGHTMAP_ON				= "LIGHTMAP_ON";
		//public static readonly string DIRLIGHTMAP_COMBINED		= "DIRLIGHTMAP_COMBINED";
		//public static readonly string _DETAIL_MULX2				= "_DETAIL_MULX2";
		//public static readonly string _DETAIL_SCALED			= "_DETAIL_SCALED";
		//public static readonly string _CLEARCOAT				= "_CLEARCOAT";
		//public static readonly string _CLEARCOATMAP				= "_CLEARCOATMAP";

		// XR
		public static readonly string UseDrawProcedural = "_USE_DRAW_PROCEDURAL";
	}
}
