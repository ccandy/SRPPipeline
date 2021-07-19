using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class RenderObjectPass : RenderPass
	{
		bool m_IsOpaque;

		public bool isRenderToCameraTarget = true;

		public RenderStateBlock stateBlock;

		public RenderObjectPass(RenderPassBlock bindBlock, string passName, int queueID, bool isOpaque, int layerMask = -1 ) : base(bindBlock, passName, queueID)
		{
			filteringSettings = new FilteringSettings(isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent, layerMask);
			m_IsOpaque = isOpaque;
			profilingSampler = new ProfilingSampler("RenderObjectPass");

			m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			m_ShaderTagIdList.Add(PassShaderTag);

			stateBlock = new RenderStateBlock();
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{

			CommandBuffer cmd = CommandBufferPool.Get();
			using var scope = new ProfilingScope(null, profilingSampler);

			float flipSign = (renderingData.cameraData.IsCameraProjectionMatrixFlipped()) ? -1.0f : 1.0f;
			Vector4 scaleBias = (flipSign < 0.0f)
				? new Vector4(flipSign, 1.0f, -1.0f, 1.0f)
				: new Vector4(flipSign, 0.0f, 1.0f, 1.0f);

			var sortFlags = (m_IsOpaque) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;

			var drawSetting = CustomRenderPipeline.CreateDrawingSettings( CustomRenderPipeline.PipelineShaderTag, 
															ref renderingData, sortFlags);

			for (int i = 0; i < m_ShaderTagIdList.Count; ++i)
			{
				drawSetting.SetShaderPassName(i, m_ShaderTagIdList[i]);
			}

			stateBlock.stencilState = stencilState;

			context.DrawRenderers(renderingData.cullResults, ref drawSetting, ref filteringSettings, ref stateBlock);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override bool IsMainCameraTargetPass()
		{
			return isRenderToCameraTarget;
		}
	}
}
