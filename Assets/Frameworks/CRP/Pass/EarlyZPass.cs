using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class EarlyZPass : RenderPass
	{
		

		public EarlyZPass(RenderPassBlock bindBlock) : base( bindBlock, "EarlyZ", (int)RenderPassQueue.EarlyZ)
		{
			profilingSampler = new ProfilingSampler("EarlyZPass");
			filteringSettings = new FilteringSettings( RenderQueueRange.opaque );
		}

		public override void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraBegin(context, ref renderingData);

			//CommandBuffer cmd = CommandBufferPool.Get();

			//m_DepthTarget = new RenderTargetIdentifier(EarlyZConstantBuffer._DepthTexture);

			//cmd.GetTemporaryRT( EarlyZConstantBuffer._DepthTexture, 
			//	renderingData.cameraData.cameraTargetDescriptor.width,
			//	renderingData.cameraData.cameraTargetDescriptor.height,
			//	32, FilterMode.Point, RenderTextureFormat.Depth);

			//context.ExecuteCommandBuffer(cmd);

			//CommandBufferPool.Release(cmd);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, profilingSampler))
			{
				var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
				var drawSettings = CustomRenderPipeline.CreateDrawingSettings(PassShaderTag, ref renderingData, sortFlags);
				drawSettings.perObjectData = PerObjectData.None;

				context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

			}
			context.ExecuteCommandBuffer(cmd);

			CommandBufferPool.Release(cmd);
		}

		public override void OnCameraEnd(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraEnd(context, ref renderingData);

			//var cmd = CommandBufferPool.Get();

			//cmd.ReleaseTemporaryRT(EarlyZConstantBuffer._DepthTexture);

			//context.ExecuteCommandBuffer(cmd);
			//CommandBufferPool.Release(cmd);
		}

		//public override void SetupTarget(ScriptableRenderContext context, ref RenderingData renderingData, ClearFlag clearFlag)
		//{
		//	CommandBuffer cmd = CommandBufferPool.Get();
		//	using var scope = new ProfilingScope(null, profilingSampler);

		//	RenderBaseFunction.SetRenderTarget(cmd, ref renderingData.curCameraColorIdentifier, ref renderingData.curCameraDepthIdentifier, ClearFlag.All, Color.clear);

		//	context.ExecuteCommandBuffer(cmd);

		//	CommandBufferPool.Release(cmd);
		//}

		public override bool IsMainCameraTargetPass()
		{
			return true;
		}
	}
}
