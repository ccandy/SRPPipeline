using System;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Frameworks.CRP
{
	public class CustomRenderPipeline : RenderPipeline
	{
		#region Profiling Info
		readonly static ProfilingSampler beginFrameRenderingProfile		= new ProfilingSampler("BeginFrameRendering");
		readonly static ProfilingSampler endFrameRenderingProfile		= new ProfilingSampler("EndFrameRendering");

		readonly static ProfilingSampler beginCameraRenderingProfile	= new ProfilingSampler("BeginCameraRendering");
		readonly static ProfilingSampler endCameraRenderingProfile		= new ProfilingSampler("EndCameraRendering");

		readonly static ProfilingSampler setupPerFrameShaderConstants	= new ProfilingSampler("SetupPerFrameShaderConstants");

		readonly static ProfilingSampler setupPerCameraShaderConstants	= new ProfilingSampler("SetupPerCameraShaderConstants");
		#endregion

		List<RenderPassBlock> m_RenderPassBlocks = new List<RenderPassBlock>();

		public readonly static ShaderTagId PipelineShaderTag = new ShaderTagId("CustomRenderPipeline");

		public static CustomRenderPipelineAsset bindAsset = null;

		public readonly static float maxShadowBias = 10.0f;
		
		public readonly static float minRenderScale = 0.1f;

		public readonly static float maxRenderScale = 2.0f;

		public CustomRenderPipeline(CustomRenderPipelineAsset asset)
		{
			bindAsset = asset;

			int qualitySettingsMsaaSampleCount = QualitySettings.antiAliasing > 0 ? QualitySettings.antiAliasing : 1;
			bool msaaSampleCountNeedsUpdate = qualitySettingsMsaaSampleCount != (int)asset.MSAASampleCount;

			if (msaaSampleCountNeedsUpdate)
			{
				QualitySettings.antiAliasing = (int)asset.MSAASampleCount;
#if ENABLE_VR && ENABLE_XR_MODULE
                XRSystem.UpdateMSAALevel(asset.msaaSampleCount);
#endif
			}

			GraphicsSettings.lightsUseLinearIntensity				= (QualitySettings.activeColorSpace == ColorSpace.Linear);
			GraphicsSettings.useScriptableRenderPipelineBatching	= asset.UseSRPBatcher;

			ResetRenderPassBlocks();

			Shader.globalRenderPipeline = "Custom Render Pipeline";

			//m_CameraColorAttachment.Init("_CameraColorTexture");
			//m_CameraDepthAttachment.Init("_CameraDepthAttachment");
			//m_DepthTexture.Init("_CameraDepthTexture");
			//m_NormalsTexture.Init("_CameraNormalsTexture");
		}


		public void ClearRenderPassBlocks()
		{
			for (int i = 0; i < m_RenderPassBlocks.Count; ++i)
			{
				var block = m_RenderPassBlocks[i];

				block.Dispose();
			}

			m_RenderPassBlocks.Clear();
		}

		public void ResetRenderPassBlocks()
		{
			if (bindAsset == null)
				return;

			ClearRenderPassBlocks();

			for (int i = 0; i < bindAsset.renderPassBlockAssets.Count; ++i)
			{
				var asset = bindAsset.renderPassBlockAssets[i];

				if (asset == null)
					continue;

				m_RenderPassBlocks.Add( bindAsset.renderPassBlockAssets[i].CreatePassBlock() );
			}
		}


#if UNITY_2021_1_OR_NEWER
        protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
#else
		protected override void Render(ScriptableRenderContext context, Camera[] cameras)
#endif
		{
#if UNITY_2021_1_OR_NEWER
            using (new ProfilingScope(null, beginFrameRenderingProfile))
            {
                BeginContextRendering(renderContext, cameras);
            }
#else
			using (new ProfilingScope(null, beginFrameRenderingProfile))
			{
				BeginFrameRendering(context, cameras);

				for (int i = 0; i < m_RenderPassBlocks.Count; ++i)
				{
					m_RenderPassBlocks[i].OnFrameBegin();
				}
			}
#endif
			GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);
			GraphicsSettings.useScriptableRenderPipelineBatching = bindAsset.UseSRPBatcher;

			SetupPerFrameShaderConstants();

//#if ENABLE_VR && ENABLE_XR_MODULE
//            // Update XR MSAA level per frame.
//            XRSystem.UpdateMSAALevel(asset.msaaSampleCount);
//#endif

			SortCameras(cameras);

#if UNITY_2021_1_OR_NEWER
            for (int i = 0; i < cameras.Count; ++i)
#else
			for (int i = 0; i < cameras.Length; ++i)
#endif
			{
				var camera = cameras[i];

#if UNITY_EDITOR
				if ( camera.cameraType == CameraType.Game || camera.cameraType == CameraType.VR)
				{
#endif
					RenderStackCamera(context, camera);
#if UNITY_EDITOR
				}
				else
				{
					RenderSceneViewCamera(context, camera);	
				}
#endif
			}

#if UNITY_2021_1_OR_NEWER
            using (new ProfilingScope(null, endFrameRenderingProfile))
            {
                EndContextRendering(renderContext, cameras);
            }
#else
			using (new ProfilingScope(null, endFrameRenderingProfile))
			{
				for (int i = 0; i < m_RenderPassBlocks.Count; ++i)
				{
					m_RenderPassBlocks[i].OnFrameEnd();
				}

				EndFrameRendering(context, cameras);
			}
#endif
		}

		public void RenderStackCamera(ScriptableRenderContext context, Camera camera)
		{
			PipelineAdditionalCameraData additionalCameraData = camera.GetPipelineAdditionalCameraData();

			if (additionalCameraData.IsOverlayCamera)
				return;

			var renderingData = RenderingData.NewRenderingData(context, camera, additionalCameraData);

			CommandBuffer cmd = CommandBufferPool.Get();

			SetupPerCameraShaderConstants(cmd, ref renderingData);

			context.ExecuteCommandBuffer(cmd);

			CommandBufferPool.Release(cmd);

			if (m_RenderPassBlocks.Count <= renderingData.cameraData.renderPassBlockIndex)
			{
				return;
			}

			var curRenderPassBlock = m_RenderPassBlocks[renderingData.cameraData.renderPassBlockIndex];

			using (new ProfilingScope(null, beginCameraRenderingProfile))
			{
				BeginCameraRendering(context, camera);
				curRenderPassBlock.OnCameraBegin(context, ref renderingData);
			}

			curRenderPassBlock.Execute(context, ref renderingData);
			context.Submit();

			using (new ProfilingScope(null, endCameraRenderingProfile))
			{
				curRenderPassBlock.OnCameraEnd(context, ref renderingData);
				EndCameraRendering(context, camera);
			}

			for ( int i = 0; i < additionalCameraData.StackCameras.Count; ++i)
			{
				var stackCamera = additionalCameraData.StackCameras[i];
				PipelineAdditionalCameraData stackAdditionalCameraData = stackCamera.GetPipelineAdditionalCameraData();
				var stackRenderingData = RenderingData.NewRenderingData(context, stackCamera, stackAdditionalCameraData);

				cmd = CommandBufferPool.Get();
				SetupPerCameraShaderConstants(cmd, ref renderingData);
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);

				if (m_RenderPassBlocks.Count <= stackRenderingData.cameraData.renderPassBlockIndex)
				{
					continue;
				}

				curRenderPassBlock = m_RenderPassBlocks[renderingData.cameraData.renderPassBlockIndex];

				using (new ProfilingScope(null, beginCameraRenderingProfile))
				{
					BeginCameraRendering(context, stackCamera);
					curRenderPassBlock.OnCameraBegin(context, ref stackRenderingData);
				}

				curRenderPassBlock.Execute(context, ref stackRenderingData);
				context.Submit();

				using (new ProfilingScope(null, endCameraRenderingProfile))
				{
					curRenderPassBlock.OnCameraEnd(context, ref renderingData);
					EndCameraRendering(context, stackCamera);
				}
			}

			//context.ExecuteCommandBuffer(cmd);
			//CommandBufferPool.Release(cmd);
		}

#if UNITY_EDITOR
	public void RenderSceneViewCamera(ScriptableRenderContext context, Camera camera)
		{
			PipelineAdditionalCameraData additionalCameraData = camera.GetPipelineAdditionalCameraData();

			if (additionalCameraData.IsOverlayCamera)
				return;

			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);

			var renderingData = RenderingData.NewRenderingData(context, camera, additionalCameraData);

			if (m_RenderPassBlocks.Count <= renderingData.cameraData.renderPassBlockIndex)
			{
				return;
			}

			var curRenderPassBlock = m_RenderPassBlocks[renderingData.cameraData.renderPassBlockIndex];

			using (new ProfilingScope(null, beginCameraRenderingProfile))
			{
				BeginCameraRendering(context, camera);

				curRenderPassBlock.OnCameraBegin(context, ref renderingData);
			}

			CommandBuffer cmd = CommandBufferPool.Get();

			SetupPerCameraShaderConstants( cmd, ref renderingData);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);


			curRenderPassBlock.Execute(context, ref renderingData);

			context.Submit();

			using (new ProfilingScope(null, endCameraRenderingProfile))
			{
				curRenderPassBlock.OnCameraEnd(context, ref renderingData);
				EndCameraRendering(context, camera);
			}
		
		}
#endif
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			Shader.globalRenderPipeline = "";
			SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
			//ShaderData.instance.Dispose();
			//DeferredShaderData.instance.Dispose();

#if ENABLE_VR && ENABLE_XR_MODULE
            m_XRSystem?.Dispose();
#endif

#if UNITY_EDITOR
			//SceneViewDrawMode.ResetDrawMode();
#endif
			//Lightmapping.ResetDelegate();
			//CameraCaptureBridge.enabled = false;
		}

		Comparison<Camera> cameraComparison = (camera1, camera2) => { return (int)camera1.depth - (int)camera2.depth; };
#if UNITY_2021_1_OR_NEWER
        void SortCameras(List<Camera> cameras)
        {
            if (cameras.Count > 1)
                cameras.Sort(cameraComparison);
        }
#else
		void SortCameras(Camera[] cameras)
		{
			if (cameras.Length > 1)
			{
				Array.Sort(cameras, cameraComparison);
			}
		}
#endif

		static void SetupPerFrameShaderConstants()
		{
			using var profScope = new ProfilingScope(null, setupPerFrameShaderConstants);

			// When glossy reflections are OFF in the shader we set a constant color to use as indirect specular
			SphericalHarmonicsL2 ambientSH = RenderSettings.ambientProbe;
			Color linearGlossyEnvColor = new Color(ambientSH[0, 0], ambientSH[1, 0], ambientSH[2, 0]) * RenderSettings.reflectionIntensity;
			Color glossyEnvColor = CoreUtils.ConvertLinearToActiveColorSpace(linearGlossyEnvColor);
			Shader.SetGlobalVector(ShaderPropertyID.glossyEnvironmentColor, glossyEnvColor);

			// Ambient
			Shader.SetGlobalVector(ShaderPropertyID.ambientSkyColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientSkyColor));
			Shader.SetGlobalVector(ShaderPropertyID.ambientEquatorColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientEquatorColor));
			Shader.SetGlobalVector(ShaderPropertyID.ambientGroundColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientGroundColor));

			// Used when subtractive mode is selected
			Shader.SetGlobalVector(ShaderPropertyID.subtractiveShadowColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.subtractiveShadowColor));

			// Required for 2D Unlit Shadergraph master node as it doesn't currently support hidden properties.
			//Shader.SetGlobalColor(ShaderPropertyID.rendererColor, Color.white);

			SetShaderTimeValues();

			SetGlobeEnvValues();
		}

		static void SetShaderTimeValues()
		{
#if UNITY_EDITOR
			float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
#else
            float time = Time.time;
#endif
			float deltaTime = Time.deltaTime;
			float smoothDeltaTime = Time.smoothDeltaTime;

			float timeEights = time / 8f;
			float timeFourth = time / 4f;
			float timeHalf = time / 2f;

			// Time values
			Vector4 timeVector = time * new Vector4(1f / 20f, 1f, 2f, 3f);
			Vector4 sinTimeVector = new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time));
			Vector4 cosTimeVector = new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time));
			Vector4 deltaTimeVector = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
			Vector4 timeParametersVector = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f);

			Shader.SetGlobalVector(ShaderPropertyID.time, timeVector);
			Shader.SetGlobalVector(ShaderPropertyID.sinTime, sinTimeVector);
			Shader.SetGlobalVector(ShaderPropertyID.cosTime, cosTimeVector);
			Shader.SetGlobalVector(ShaderPropertyID.deltaTime, deltaTimeVector);
			Shader.SetGlobalVector(ShaderPropertyID.timeParameters, timeParametersVector);
		}

		static void SetGlobeEnvValues()
		{
			if (bindAsset.EnvironmentType == EnvType.GLOBAL_CUBE && bindAsset.GlobalCubeMap != null)
			{
				Shader.SetGlobalTexture(ShaderPropertyID.EnvCubeMap, bindAsset.GlobalCubeMap);
				
				Shader.SetGlobalFloat(ShaderPropertyID.EnvironmentExposure, 1.0f);
				Shader.SetGlobalFloat(ShaderPropertyID.MaxLod, bindAsset.GlobalCubeMap.mipmapCount);
				Shader.SetGlobalVector(ShaderPropertyID.SHAr, bindAsset.CubeSH9.SHParams[0]);
				Shader.SetGlobalVector(ShaderPropertyID.SHAg, bindAsset.CubeSH9.SHParams[1]);
				Shader.SetGlobalVector(ShaderPropertyID.SHAb, bindAsset.CubeSH9.SHParams[2]);
				Shader.SetGlobalVector(ShaderPropertyID.SHBr, bindAsset.CubeSH9.SHParams[3]);
				Shader.SetGlobalVector(ShaderPropertyID.SHBg, bindAsset.CubeSH9.SHParams[4]);
				Shader.SetGlobalVector(ShaderPropertyID.SHBb, bindAsset.CubeSH9.SHParams[5]);
				Shader.SetGlobalVector(ShaderPropertyID.SHC,  bindAsset.CubeSH9.SHParams[6]);
			}
		}

		static void SetLightingShaderConstants(CommandBuffer cmd, ref RenderingData renderingData)
		{
			int mainLightIndex = renderingData.lightData.mainLightIndex;
			if (mainLightIndex == -1
				|| mainLightIndex >= LightData.Lights.Length)
			{
				cmd.SetGlobalVector(ShaderPropertyID.MainLightDirection, Vector4.zero);
				cmd.SetGlobalVector(ShaderPropertyID.MainLightColor, Vector4.zero);
				return;
			}

			ref VisibleLight light = ref LightData.Lights[mainLightIndex];

			Vector4 dir = -light.localToWorldMatrix.GetColumn(2);
			

			cmd.SetGlobalVector(ShaderPropertyID.MainLightDirection, new Vector4(dir.x, dir.y, dir.z, 0.0f));
			cmd.SetGlobalVector(ShaderPropertyID.MainLightColor, light.finalColor);

			//CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadows)
		}

		static void SetupPerCameraShaderConstants(CommandBuffer cmd, ref RenderingData renderingData)
		{
			using var profScope = new ProfilingScope(null, setupPerCameraShaderConstants);

			ref CameraData cameraData = ref renderingData.cameraData;

			Camera camera = cameraData.camera;

			Rect pixelRect = cameraData.pixelRect;
			float renderScale = cameraData.renderScale; /*cameraData.isSceneViewCamera ? 1f : cameraData.renderScale;*/
			float scaledCameraWidth = (float)pixelRect.width * renderScale;
			float scaledCameraHeight = (float)pixelRect.height * renderScale;
			float cameraWidth = (float)pixelRect.width;
			float cameraHeight = (float)pixelRect.height;

			// Use eye texture's width and height as screen params when XR is enabled
			//if (cameraData.xr.enabled)
			//{
			//	scaledCameraWidth = (float)cameraData.cameraTargetDescriptor.width;
			//	scaledCameraHeight = (float)cameraData.cameraTargetDescriptor.height;
			//	cameraWidth = (float)cameraData.cameraTargetDescriptor.width;
			//	cameraHeight = (float)cameraData.cameraTargetDescriptor.height;
			//}

			if (camera.allowDynamicResolution)
			{
				scaledCameraWidth *= ScalableBufferManager.widthScaleFactor;
				scaledCameraHeight *= ScalableBufferManager.heightScaleFactor;
			}

			float near = camera.nearClipPlane;
			float far = camera.farClipPlane;
			float invNear = Mathf.Approximately(near, 0.0f) ? 0.0f : 1.0f / near;
			float invFar = Mathf.Approximately(far, 0.0f) ? 0.0f : 1.0f / far;
			float isOrthographic = camera.orthographic ? 1.0f : 0.0f;

			// From http://www.humus.name/temp/Linearize%20depth.txt
			// But as depth component textures on OpenGL always return in 0..1 range (as in D3D), we have to use
			// the same constants for both D3D and OpenGL here.
			// OpenGL would be this:
			// zc0 = (1.0 - far / near) / 2.0;
			// zc1 = (1.0 + far / near) / 2.0;
			// D3D is this:
			float zc0 = 1.0f - far * invNear;
			float zc1 = far * invNear;

			Vector4 zBufferParams = new Vector4(zc0, zc1, zc0 * invFar, zc1 * invFar);

			if (SystemInfo.usesReversedZBuffer)
			{
				zBufferParams.y += zBufferParams.x;
				zBufferParams.x = -zBufferParams.x;
				zBufferParams.w += zBufferParams.z;
				zBufferParams.z = -zBufferParams.z;
			}

			// Projection flip sign logic is very deep in GfxDevice::SetInvertProjectionMatrix
			// For now we don't deal with _ProjectionParams.x and let SetupCameraProperties handle it.
			// We need to enable this when we remove SetupCameraProperties
			// float projectionFlipSign = ???
			// Vector4 projectionParams = new Vector4(projectionFlipSign, near, far, 1.0f * invFar);
			// cmd.SetGlobalVector(ShaderPropertyId.projectionParams, projectionParams);

			Vector4 orthoParams = new Vector4(camera.orthographicSize * cameraData.aspectRatio, camera.orthographicSize, 0.0f, isOrthographic);

			// Camera and Screen variables as described in https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
			cmd.SetGlobalVector(ShaderPropertyID.worldSpaceCameraPos, camera.transform.position);
			cmd.SetGlobalVector(ShaderPropertyID.screenParams, new Vector4(cameraWidth, cameraHeight, 1.0f + 1.0f / cameraWidth, 1.0f + 1.0f / cameraHeight));
			cmd.SetGlobalVector(ShaderPropertyID.scaledScreenParams, new Vector4(scaledCameraWidth, scaledCameraHeight, 1.0f + 1.0f / scaledCameraWidth, 1.0f + 1.0f / scaledCameraHeight));
			cmd.SetGlobalVector(ShaderPropertyID.zBufferParams, zBufferParams);
			cmd.SetGlobalVector(ShaderPropertyID.orthoParams, orthoParams);

			SetLightingShaderConstants( cmd, ref renderingData);
		}

		public static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, float renderScale,
			bool isHdrEnabled, int msaaSamples, bool needsAlpha)
		{
			RenderTextureDescriptor desc;
			GraphicsFormat renderTextureFormatDefault = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);

			if (camera.targetTexture == null)
			{
				desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
				desc.width = (int)((float)desc.width * renderScale);
				desc.height = (int)((float)desc.height * renderScale);


				GraphicsFormat hdrFormat;
				if (!needsAlpha && RenderBaseFunction.SupportsGraphicsFormat(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Linear | FormatUsage.Render))
					hdrFormat = GraphicsFormat.B10G11R11_UFloatPack32;
				else if (RenderBaseFunction.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Linear | FormatUsage.Render))
					hdrFormat = GraphicsFormat.R16G16B16A16_SFloat;
				else
					hdrFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR); // This might actually be a LDR format on old devices.

				desc.graphicsFormat = isHdrEnabled ? hdrFormat : renderTextureFormatDefault;
				desc.depthBufferBits = 32;
				desc.msaaSamples = msaaSamples;
				desc.sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
			}
			else
			{
				desc = camera.targetTexture.descriptor;
				desc.width = camera.pixelWidth;
				desc.height = camera.pixelHeight;
				if (camera.cameraType == CameraType.SceneView && !isHdrEnabled)
				{
					desc.graphicsFormat = renderTextureFormatDefault;
				}
				// SystemInfo.SupportsRenderTextureFormat(camera.targetTexture.descriptor.colorFormat)
				// will assert on R8_SINT since it isn't a valid value of RenderTextureFormat.
				// If this is fixed then we can implement debug statement to the user explaining why some
				// RenderTextureFormats available resolves in a black render texture when no warning or error
				// is given.
			}

			desc.enableRandomWrite = false;
			desc.bindMS = false;
			desc.useDynamicScale = camera.allowDynamicResolution;

			// check that the requested MSAA samples count is supported by the current platform. If it's not supported,
			// replace the requested desc.msaaSamples value with the actual value the engine falls back to
			desc.msaaSamples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(desc);

			return desc;
		}

		public static DrawingSettings CreateDrawingSettings(ShaderTagId shaderTagId, ref RenderingData renderingData, SortingCriteria sortingCriteria)
		{
			Camera camera = renderingData.cameraData.camera;
			SortingSettings sortingSettings = new SortingSettings(camera) { criteria = sortingCriteria };
			DrawingSettings settings = new DrawingSettings(shaderTagId, sortingSettings)
			{
				perObjectData = renderingData.perObjectData,
				mainLightIndex = renderingData.lightData.mainLightIndex,
				enableDynamicBatching = renderingData.supportsDynamicBatching,

				// Disable instancing for preview cameras. This is consistent with the built-in forward renderer. Also fixes case 1127324.
				enableInstancing = camera.cameraType == CameraType.Preview ? false : true,
			};
			return settings;
		}

		

	}

}
