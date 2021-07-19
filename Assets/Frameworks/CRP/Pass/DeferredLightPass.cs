using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class DeferredLightPass : RenderPass
	{
		Matrix4x4 zScaleBias = Matrix4x4.identity;

		Matrix4x4 m_ScreenToWorld = Matrix4x4.identity;

		Material m_DeferredLightingMaterial = new Material(Shader.Find("Hidden/CustomRenderPipeline/DeferredLighting"));

		#region LightData To Shader
		const int MaxDirectionLightPerPassCount		= 8;
		int			_DirectionalLight_DealedIndices = 0;
		int			_DirectionalLight_Count			= 0;
		Vector4[]	_DirectionalLight_Directions	= new Vector4[MaxDirectionLightPerPassCount];
		Vector4[]	_DirectionalLight_Colores		= new Vector4[MaxDirectionLightPerPassCount];

		const int MaxPointLightPerPassCount			= 8;
		int			_PointLight_DealedIndices		= 0;
		int			_PointLight_Count				= 0;
		Vector4[]	_PointLight_Positions			= new Vector4[MaxPointLightPerPassCount];
		Vector4[]	_PointLight_Colores				= new Vector4[MaxPointLightPerPassCount];
		float[]		_PointLight_Range				= new float[MaxPointLightPerPassCount];
		#endregion


		public static class ShaderConstants
		{
			public static readonly int _ScreenToWorld				= Shader.PropertyToID("_ScreenToWorld");

			public static readonly int _DirectionalLight_Count		= Shader.PropertyToID("_DirectionalLight_Count");
			public static readonly int _DirectionalLight_Directions	= Shader.PropertyToID("_DirectionalLight_Directions");
			public static readonly int _DirectionalLight_Colores	= Shader.PropertyToID("_DirectionalLight_Colores");

			public static readonly int _PointLight_Count			= Shader.PropertyToID("_PointLight_Count");
			public static readonly int _PointLight_Positions		= Shader.PropertyToID("_PointLight_Positions");
			public static readonly int _PointLight_Colores			= Shader.PropertyToID("_PointLight_Colores");
			public static readonly int _PointLight_Range			= Shader.PropertyToID("_PointLight_Range");
		}

		public DeferredLightPass( RenderPassBlock bindBlock ) : base (bindBlock,"DeferredLight", (int)RenderPassQueue.DeferredLighting)
		{
			profilingSampler = new ProfilingSampler(nameof(DeferredLightPass));
			filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

			if (RenderBaseFunction.IsGLRenderer())
			{
				// We need to manunally adjust z in NDC space from [-1; 1] to [0; 1] (storage in depth texture).
				zScaleBias = new Matrix4x4(
						new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
						new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
						new Vector4(0.0f, 0.0f, 0.5f, 0.0f),
						new Vector4(0.0f, 0.0f, 0.5f, 1.0f)
					);
			}
		}

		public void SetupMatrixConstants(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ref CameraData cameraData = ref renderingData.cameraData;

			Matrix4x4 proj = cameraData.projMatrix;
			Matrix4x4 view = cameraData.viewMatrix;
			Matrix4x4 gpuProj = GL.GetGPUProjectionMatrix(proj, false);

			// xy coordinates in range [-1; 1] go to pixel coordinates.
			Matrix4x4 toScreen = new Matrix4x4(
				new Vector4(0.5f * cameraData.cameraTargetDescriptor.width, 0.0f, 0.0f, 0.0f),
				new Vector4(0.0f, 0.5f * cameraData.cameraTargetDescriptor.height, 0.0f, 0.0f),
				new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
				new Vector4(0.5f * cameraData.cameraTargetDescriptor.width, 0.5f * cameraData.cameraTargetDescriptor.height, 0.0f, 1.0f)
			);

			m_ScreenToWorld = Matrix4x4.Inverse(toScreen * zScaleBias * gpuProj * view);
			

			cmd.SetGlobalMatrix(ShaderConstants._ScreenToWorld, m_ScreenToWorld);
		}

		public void InitLightingData()
		{
			_DirectionalLight_DealedIndices = 0;
			_DirectionalLight_Count			= 0;

			_PointLight_DealedIndices		= 0;
			_PointLight_Count				= 0;
		}

		void SetupMainLightShadowData(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ref LightData lightData = ref renderingData.lightData;

			if (lightData.mainLightIndex != -1)
			{
				bool softShadows = LightData.Lights[lightData.mainLightIndex].light.shadows == LightShadows.Soft && renderingData.shadowData.isUseSoftShadowMap;

				CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadows, LightData.Lights[lightData.mainLightIndex].light.shadows != LightShadows.None);
				CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadowCascades, renderingData.shadowData.shadowCascadesCount > 1);
				CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, softShadows);
			}

			CoreUtils.SetKeyword(cmd, ShaderKeywordStrings._USE_ENV_CUBE, CustomRenderPipeline.bindAsset.EnvironmentType == EnvType.GLOBAL_CUBE);
			
		}

		void ColletDirectionalLightData(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ref LightData lightData = ref renderingData.lightData;

			_DirectionalLight_Count = 0;

			for (int i = 0; i < lightData.DirectinalLightCount && i < MaxDirectionLightPerPassCount; ++i)
			{
				int realIndex = _DirectionalLight_DealedIndices;
				VisibleLight light = LightData.Lights[LightData.DirectinalLightIndices[realIndex]];

				_DirectionalLight_Directions[_DirectionalLight_Count] = -light.localToWorldMatrix.GetColumn(2);
				_DirectionalLight_Colores[_DirectionalLight_Count] = light.finalColor;
				++_DirectionalLight_DealedIndices;
				++_DirectionalLight_Count;
			}

			cmd.SetGlobalInt(ShaderConstants._DirectionalLight_Count, _DirectionalLight_Count);
			cmd.SetGlobalVectorArray(ShaderConstants._DirectionalLight_Directions, _DirectionalLight_Directions);
			cmd.SetGlobalVectorArray(ShaderConstants._DirectionalLight_Colores, _DirectionalLight_Colores);
		}

		void ColletPointLightData(CommandBuffer cmd, ref RenderingData renderingData)
		{
			ref LightData lightData = ref renderingData.lightData;

			_PointLight_Count = 0;
			for (int i = 0; i < lightData.PointLightCount && i < MaxPointLightPerPassCount; ++i)
			{
				int realIndex = _PointLight_DealedIndices;
				VisibleLight light = LightData.Lights[LightData.PointLightIndices[realIndex]];

				_PointLight_Positions[_PointLight_Count]	= light.localToWorldMatrix.GetColumn(3);
				_PointLight_Colores[_PointLight_Count]		= light.finalColor;
				_PointLight_Range[_PointLight_Count]		= light.range;
				++_PointLight_DealedIndices;
				++_PointLight_Count;
			}
			cmd.SetGlobalInt(ShaderConstants._PointLight_Count, _PointLight_Count);
			cmd.SetGlobalVectorArray(ShaderConstants._PointLight_Positions, _PointLight_Positions);
			cmd.SetGlobalVectorArray(ShaderConstants._PointLight_Colores, _PointLight_Colores);
			cmd.SetGlobalFloatArray(ShaderConstants._PointLight_Range, _PointLight_Range);
		}

		public void SetupLightingData(CommandBuffer cmd, ref RenderingData renderingData)
		{
			
			

			
		}

		public override void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraBegin(context, ref renderingData);
			InitLightingData();
			var cmd = CommandBufferPool.Get();
			SetupMatrixConstants(cmd, ref renderingData);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, profilingSampler))
			{
				SetupMainLightShadowData(cmd, ref renderingData);
				ColletDirectionalLightData(cmd, ref renderingData);
				ColletPointLightData(cmd, ref renderingData);
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				cmd.DrawMesh( RenderBaseFunction.fullscreenMesh, Matrix4x4.identity, m_DeferredLightingMaterial, 0, 0);
				cmd.DrawMesh(RenderBaseFunction.fullscreenMesh, Matrix4x4.identity, m_DeferredLightingMaterial, 0, 2);

				while (_DirectionalLight_DealedIndices < renderingData.lightData.DirectinalLightCount)
				{
					ColletDirectionalLightData(cmd, ref renderingData);
					cmd.DrawMesh(RenderBaseFunction.fullscreenMesh, Matrix4x4.identity, m_DeferredLightingMaterial, 0, 1);
				}

				while (_PointLight_DealedIndices < renderingData.lightData.PointLightCount)
				{
					ColletPointLightData(cmd, ref renderingData);
					cmd.DrawMesh(RenderBaseFunction.fullscreenMesh, Matrix4x4.identity, m_DeferredLightingMaterial, 0, 2);
				}

				
			}
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override bool IsMainCameraTargetPass()
		{
			return true;
		}
	}
}