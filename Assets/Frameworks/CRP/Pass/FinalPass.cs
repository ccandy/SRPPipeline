using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class FinalPass : RenderPass
	{
		Material finalPassMaterial = new Material(Shader.Find("Hidden/CustomRenderPipeline/Blit"));

		public FinalPass(RenderPassBlock bindBlock) : base(bindBlock, "Final", (int)RenderPassQueue.FinalPass)
		{
			profilingSampler = new ProfilingSampler(nameof(FinalPass));
			filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		}

		public override void SetupTarget(ScriptableRenderContext context, ref RenderingData renderingData, ClearFlag clearFlag)
		{
			var cmd = CommandBufferPool.Get();
			using var scope = new ProfilingScope(cmd, profilingSampler);

			RenderBaseFunction.SetRenderTarget(cmd, ref renderingData.orignCameraColorIdentifier, ref renderingData.orignCameraDepthIdentifier, ClearFlag.Color, renderingData.cameraData.clearColor);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, profilingSampler))
			{
				var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
				var drawSettings = CustomRenderPipeline.CreateDrawingSettings(PassShaderTag, ref renderingData, sortFlags);
				drawSettings.perObjectData = PerObjectData.None;

				RenderBaseFunction.Blit(cmd, renderingData.curCameraColorIdentifier, renderingData.orignCameraColorIdentifier, finalPassMaterial);

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
