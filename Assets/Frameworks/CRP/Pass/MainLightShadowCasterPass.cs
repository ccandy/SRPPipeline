using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class MainLightShadowCasterPass : RenderPass
	{
		FilteringSettings filteringSettings;

		public static class MainLightShadowConstantBuffer
		{
			public static int _WorldToShadow						= Shader.PropertyToID("_MainLightWorldToShadow");
			public static int _ShadowParams							= Shader.PropertyToID("_MainLightShadowParams");
			public static int _CascadeShadowSplitSpheres0			= Shader.PropertyToID("_CascadeShadowSplitSpheres0");
			public static int _CascadeShadowSplitSpheres1			= Shader.PropertyToID("_CascadeShadowSplitSpheres1");
			public static int _CascadeShadowSplitSpheres2			= Shader.PropertyToID("_CascadeShadowSplitSpheres2");
			public static int _CascadeShadowSplitSpheres3			= Shader.PropertyToID("_CascadeShadowSplitSpheres3");
			public static int _CascadeShadowSplitSphereRadii		= Shader.PropertyToID("_CascadeShadowSplitSphereRadii");
			public static int _ShadowOffset0						= Shader.PropertyToID("_MainLightShadowOffset0");
			public static int _ShadowOffset1						= Shader.PropertyToID("_MainLightShadowOffset1");
			public static int _ShadowOffset2						= Shader.PropertyToID("_MainLightShadowOffset2");
			public static int _ShadowOffset3						= Shader.PropertyToID("_MainLightShadowOffset3");
			public static int _ShadowmapSize						= Shader.PropertyToID("_MainLightShadowmapSize");

			public static int _MainLightShadowmapTexture			= Shader.PropertyToID("_MainLightShadowmapTexture");

			public static int _ShadowBias							= Shader.PropertyToID("_ShadowBias");
			public static int _LightDirection						= Shader.PropertyToID("_LightDirection");
		}

		//public struct ShadowSliceData
		//{
		//	public Matrix4x4 viewMatrix;
		//	public Matrix4x4 projectionMatrix;
		//	public Matrix4x4 shadowTransform;
		//	public int offsetX;
		//	public int offsetY;
		//	public int resolution;

		//	public void Clear()
		//	{
		//		viewMatrix = Matrix4x4.identity;
		//		projectionMatrix = Matrix4x4.identity;
		//		shadowTransform = Matrix4x4.identity;
		//		offsetX = offsetY = 0;
		//		resolution = 1024;
		//	}
		//}

		const int k_MaxCascades = 4;
		const int k_ShadowmapBufferBits = 16;
		float	m_MaxShadowDistance;
		int		m_ShadowmapWidth;
		int		m_ShadowmapHeight;
		int		m_ShadowCasterCascadesCount;
		bool	m_SupportsBoxFilterForShadows;

		RenderTargetIdentifier	m_MainLightShadowmapTarget;
		//RenderTargetIdentifier	m_MainLightShadowmapDepthTarget;
		//RenderTexture			m_MainLightShadowmapTexture;

		Matrix4x4[]			m_MainLightShadowMatrices;
        ShadowSliceData[]	m_CascadeSlices;
        Vector4[]			m_CascadeSplitDistances;

		RenderTextureFormat m_ShadowFormat;

		bool isRenderingPerFrame = true;

		public MainLightShadowCasterPass(RenderPassBlock bindBlock, bool isOpaque = true, int layerMask = -1) : base( bindBlock, "ShadowCaster", (int)RenderPassQueue.MainLightShadowMap)
		{
			filteringSettings = new FilteringSettings(isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent, layerMask);

			profilingSampler = new ProfilingSampler("MainLightShadowCasterPass");

			m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
			m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
			m_CascadeSplitDistances = new Vector4[k_MaxCascades];

			m_ShaderTagIdList.Add(PassShaderTag);

			m_ShadowFormat = RenderBaseFunction.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap) && (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
				? RenderTextureFormat.Shadowmap
				: RenderTextureFormat.Depth;
		}

		public override void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraBegin(context, ref renderingData);

			isRenderingPerFrame = true;

			ref LightData lightData = ref renderingData.lightData;
			ref ShadowData shadowData = ref renderingData.shadowData;

			if (!shadowData.isUseShadowmap)
			{
				isRenderingPerFrame = false;
				return;
			}

			if (lightData.mainLightIndex == -1)
			{
				isRenderingPerFrame = false;
				return;
			}

			VisibleLight shadowLight = LightData.Lights[lightData.mainLightIndex];
			Light light = shadowLight.light;
			if (light.shadows == LightShadows.None)
			{
				isRenderingPerFrame = false;
				return;
			}

			Bounds bounds;
			if (!renderingData.cullResults.GetShadowCasterBounds(lightData.mainLightIndex, out bounds))
			{
				isRenderingPerFrame = false;
				return;
			}

			m_ShadowCasterCascadesCount = renderingData.shadowData.shadowCascadesCount;

			int shadowResolution = RenderBaseFunction.GetMaxTileResolutionInAtlas(renderingData.shadowData.shadowmapResolution, m_ShadowCasterCascadesCount);

			//int shadowResolution = ShadowUtils.GetMaxTileResolutionInAtlas(renderingData.shadowData.shadowmapResolution,
			//	renderingData.shadowData.shadowmapResolution, m_ShadowCasterCascadesCount);
			m_ShadowmapWidth = renderingData.shadowData.shadowmapResolution;
			m_ShadowmapHeight = (m_ShadowCasterCascadesCount == 2) ?
				renderingData.shadowData.shadowmapResolution >> 1 :
				renderingData.shadowData.shadowmapResolution;

			for (int cascadeIndex = 0; cascadeIndex < m_ShadowCasterCascadesCount; ++cascadeIndex)
			{
				bool success = RenderBaseFunction.ExtractDirectionalLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData,
					lightData.mainLightIndex, cascadeIndex, m_ShadowmapWidth, m_ShadowmapHeight, shadowResolution, light.shadowNearPlane,
					out m_CascadeSplitDistances[cascadeIndex], out m_CascadeSlices[cascadeIndex], out m_CascadeSlices[cascadeIndex].viewMatrix, out m_CascadeSlices[cascadeIndex].projectionMatrix);

				if (!success)
				{
					isRenderingPerFrame = false;
					return;
				}
			}

			m_MaxShadowDistance = CustomRenderPipeline.bindAsset.ShadowDistance * CustomRenderPipeline.bindAsset.ShadowDistance;


			//m_MainLightShadowmapTexture = RenderTexture.GetTemporary(m_ShadowmapWidth, m_ShadowmapHeight, 32, m_ShadowFormat);

			//m_MainLightShadowmapTexture.filterMode = FilterMode.Point;
			//m_MainLightShadowmapTexture.wrapMode = TextureWrapMode.Clamp;
			//m_MainLightShadowmapTarget = new RenderTargetIdentifier(m_MainLightShadowmapTexture);
			//m_MainLightShadowmapDepthTarget = BuiltinRenderTextureType.CameraTarget;

			CommandBuffer cmd = CommandBufferPool.Get();

			m_MainLightShadowmapTarget = new RenderTargetIdentifier(MainLightShadowConstantBuffer._MainLightShadowmapTexture);

			cmd.GetTemporaryRT(MainLightShadowConstantBuffer._MainLightShadowmapTexture, m_ShadowmapWidth, m_ShadowmapHeight, 32, FilterMode.Point, m_ShadowFormat);
			//cmd.GetTemporaryRT(MainLightShadowConstantBuffer._MainLightShadowmapTexture, m_ShadowmapWidth, m_ShadowmapHeight, 32, FilterMode.Point, renderingData.cameraData.cameraTargetDescriptor.graphicsFormat);

			context.ExecuteCommandBuffer(cmd);

			CommandBufferPool.Release(cmd);

		}

		public override void SetupTarget(ScriptableRenderContext context, ref RenderingData renderingData, ClearFlag clearFlag)
		{
			if (!isRenderingPerFrame)
				return;

			CommandBuffer cmd = CommandBufferPool.Get();
			using var scope = new ProfilingScope(null, profilingSampler);

			RenderBaseFunction.SetRenderTarget(cmd, ref m_MainLightShadowmapTarget, ref m_MainLightShadowmapTarget, ClearFlag.All, Color.clear);

			context.ExecuteCommandBuffer(cmd);

			CommandBufferPool.Release(cmd);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (!isRenderingPerFrame)
				return;

			ref LightData lightData		= ref renderingData.lightData;
			ref ShadowData shadowData	= ref renderingData.shadowData;

			CommandBuffer cmd = CommandBufferPool.Get();

			using var scope = new ProfilingScope(cmd, profilingSampler);

			int lightIndex = lightData.mainLightIndex;
			VisibleLight light = LightData.Lights[lightIndex];
			var drawSetting = new ShadowDrawingSettings(renderingData.cullResults, lightIndex);

			for (int cascadeIndex = 0; cascadeIndex < m_ShadowCasterCascadesCount; ++cascadeIndex)
			{
				var splitData = drawSetting.splitData;
				splitData.cullingSphere = m_CascadeSplitDistances[cascadeIndex];
				drawSetting.splitData = splitData;

				Vector4 shadowBias = RenderBaseFunction.GetShadowBias(ref light, lightIndex, ref shadowData, m_CascadeSlices[cascadeIndex].projectionMatrix, m_CascadeSlices[cascadeIndex].resolution);
				SetupShadowCasterConstantBuffer(cmd, ref light, shadowBias);

				ref ShadowSliceData shadowSliceData = ref m_CascadeSlices[cascadeIndex];
				Rect sliceRect = new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution);
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				//cmd.EnableScissorRect(sliceRect);
				cmd.SetViewport(sliceRect);
				cmd.SetViewProjectionMatrices(shadowSliceData.viewMatrix, shadowSliceData.projectionMatrix);
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				context.DrawShadows(ref drawSetting);
				cmd.DisableScissorRect();
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
			}
						
			bool softShadows = light.light.shadows == LightShadows.Soft && shadowData.isUseSoftShadowMap;

			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadows, light.light.shadows != LightShadows.None);
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadowCascades, shadowData.shadowCascadesCount > 1);
			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, softShadows);

			SetupMainLightShadowReceiverConstants(cmd, ref light, shadowData.isUseSoftShadowMap);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void OnCameraEnd(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraEnd(context, ref renderingData);

			//if (m_MainLightShadowmapTexture)
			//{
			//	RenderTexture.ReleaseTemporary(m_MainLightShadowmapTexture);
			//	m_MainLightShadowmapTexture = null;
			//}

			var cmd = CommandBufferPool.Get();

			cmd.ReleaseTemporaryRT(MainLightShadowConstantBuffer._MainLightShadowmapTexture);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override bool IsMainCameraTargetPass()
		{
			return false;
		}

		public static void SetupShadowCasterConstantBuffer(CommandBuffer cmd, ref VisibleLight shadowLight, Vector4 shadowBias)
		{
			Vector3 lightDirection = -shadowLight.localToWorldMatrix.GetColumn(2);
			cmd.SetGlobalVector( MainLightShadowConstantBuffer._ShadowBias, shadowBias);
			cmd.SetGlobalVector( MainLightShadowConstantBuffer._LightDirection, new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));
		}

		void SetupMainLightShadowReceiverConstants(CommandBuffer cmd, ref VisibleLight shadowLight, bool supportsSoftShadows)
		{
			Light light = shadowLight.light;
			bool softShadows = shadowLight.light.shadows == LightShadows.Soft && supportsSoftShadows;

			int cascadeCount = m_ShadowCasterCascadesCount;
			for (int i = 0; i < cascadeCount; ++i)
				m_MainLightShadowMatrices[i] = m_CascadeSlices[i].shadowTransform;

			// We setup and additional a no-op WorldToShadow matrix in the last index
			// because the ComputeCascadeIndex function in Shadows.hlsl can return an index
			// out of bounds. (position not inside any cascade) and we want to avoid branching
			Matrix4x4 noOpShadowMatrix = Matrix4x4.zero;
			noOpShadowMatrix.m22 = (SystemInfo.usesReversedZBuffer) ? 1.0f : 0.0f;
			for (int i = cascadeCount; i <= k_MaxCascades; ++i)
				m_MainLightShadowMatrices[i] = noOpShadowMatrix;

			float invShadowAtlasWidth = 1.0f / m_ShadowmapWidth;
			float invShadowAtlasHeight = 1.0f / m_ShadowmapHeight;
			float invHalfShadowAtlasWidth = 0.5f * invShadowAtlasWidth;
			float invHalfShadowAtlasHeight = 0.5f * invShadowAtlasHeight;
			float softShadowsProp = softShadows ? 1.0f : 0.0f;

			//To make the shadow fading fit into a single MAD instruction:
			//distanceCamToPixel2 * oneOverFadeDist + minusStartFade (single MAD)
			float startFade = m_MaxShadowDistance * 0.9f;
			float oneOverFadeDist = 1 / (m_MaxShadowDistance - startFade);
			float minusStartFade = -startFade * oneOverFadeDist;

			//cmd.SetGlobalTexture( MainLightShadowConstantBuffer._MainLightShadowmapTexture, m_MainLightShadowmapTexture);
			cmd.SetGlobalMatrixArray(MainLightShadowConstantBuffer._WorldToShadow, m_MainLightShadowMatrices);
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowParams, new Vector4(light.shadowStrength, softShadowsProp, oneOverFadeDist, minusStartFade));

			if (m_ShadowCasterCascadesCount > 1)
			{
				cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres0,
					m_CascadeSplitDistances[0]);
				cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres1,
					m_CascadeSplitDistances[1]);
				cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres2,
					m_CascadeSplitDistances[2]);
				cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres3,
					m_CascadeSplitDistances[3]);
				cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSphereRadii, new Vector4(
					m_CascadeSplitDistances[0].w * m_CascadeSplitDistances[0].w,
					m_CascadeSplitDistances[1].w * m_CascadeSplitDistances[1].w,
					m_CascadeSplitDistances[2].w * m_CascadeSplitDistances[2].w,
					m_CascadeSplitDistances[3].w * m_CascadeSplitDistances[3].w));
			}

			// Inside shader soft shadows are controlled through global keyword.
			// If any additional light has soft shadows it will force soft shadows on main light too.
			// As it is not trivial finding out which additional light has soft shadows, we will pass main light properties if soft shadows are supported.
			// This workaround will be removed once we will support soft shadows per light.
			if (supportsSoftShadows)
			{
				//if (m_SupportsBoxFilterForShadows)
				//{
				//	cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset0,
				//		new Vector4(-invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight, 0.0f, 0.0f));
				//	cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset1,
				//		new Vector4(invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight, 0.0f, 0.0f));
				//	cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset2,
				//		new Vector4(-invHalfShadowAtlasWidth, invHalfShadowAtlasHeight, 0.0f, 0.0f));
				//	cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset3,
				//		new Vector4(invHalfShadowAtlasWidth, invHalfShadowAtlasHeight, 0.0f, 0.0f));
				//}

				// Currently only used when !SHADER_API_MOBILE but risky to not set them as it's generic
				// enough so custom shaders might use it.
				cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowmapSize, new Vector4(invShadowAtlasWidth,
					invShadowAtlasHeight,
					m_ShadowmapWidth, m_ShadowmapHeight));
			}
		}

	}
}
