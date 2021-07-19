using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class GBufferPass : RenderPass
	{
		public static class GBufferConstantBuffer
		{
			public static int GBuffer0 = Shader.PropertyToID("_GBuffer0");
			public static int GBuffer1 = Shader.PropertyToID("_GBuffer1");
			public static int GBuffer2 = Shader.PropertyToID("_GBuffer2");
			public static int GBuffer3 = Shader.PropertyToID("_GBuffer3");
		}

		RenderTargetIdentifier[] GBufferIdentifiers;

		public GBufferPass(RenderPassBlock bindBlock) : base( bindBlock, "GBuffer", (int)RenderPassQueue.GBuffer)
		{
			profilingSampler = new ProfilingSampler(nameof(GBufferPass));
			filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

			GBufferIdentifiers = new RenderTargetIdentifier[]
				{
					new RenderTargetIdentifier(GBufferConstantBuffer.GBuffer0),
					new RenderTargetIdentifier(GBufferConstantBuffer.GBuffer1),
					new RenderTargetIdentifier(GBufferConstantBuffer.GBuffer2),
					new RenderTargetIdentifier(GBufferConstantBuffer.GBuffer3),
				};
		}

		public enum BufferType
		{
			LightingData,
			OrignData,
			Depth,
		}

		public static GraphicsFormat GetGBufferFormat(BufferType bufferType)
		{
			switch(bufferType)
			{
				case BufferType.LightingData:
					return QualitySettings.activeColorSpace == ColorSpace.Linear ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm;
				case BufferType.OrignData:
					//return QualitySettings.activeColorSpace == ColorSpace.Linear ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm;
					return GraphicsFormat.R8G8B8A8_UNorm; // GraphicsFormat.R8G8B8A8_SNorm;
				case BufferType.Depth:
					return GraphicsFormat.R32_SFloat;
			}

			return GraphicsFormat.None;
		}

		public override void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraBegin(context, ref renderingData);

			RenderTextureDescriptor rtDesc = renderingData.cameraData.cameraTargetDescriptor;
			rtDesc.depthBufferBits = 0;
			rtDesc.stencilFormat = GraphicsFormat.None;
			rtDesc.autoGenerateMips = false;

			var cmd = CommandBufferPool.Get();
			rtDesc.graphicsFormat = GetGBufferFormat(BufferType.LightingData);
			cmd.GetTemporaryRT(GBufferConstantBuffer.GBuffer0, rtDesc);

			//rtDesc1.sRGB = false;
			//rtDesc2.sRGB = false;
			//rtDesc3.sRGB = false;
			rtDesc.sRGB = false;
			rtDesc.graphicsFormat = GetGBufferFormat(BufferType.OrignData);
			cmd.GetTemporaryRT(GBufferConstantBuffer.GBuffer1, rtDesc);

			rtDesc.graphicsFormat = GetGBufferFormat(BufferType.OrignData);
			cmd.GetTemporaryRT(GBufferConstantBuffer.GBuffer2, rtDesc);

			rtDesc.graphicsFormat = GetGBufferFormat(BufferType.OrignData);
			cmd.GetTemporaryRT(GBufferConstantBuffer.GBuffer3, rtDesc);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void SetupTarget(ScriptableRenderContext context, ref RenderingData renderingData, ClearFlag clearFlag)
		{
			var cmd = CommandBufferPool.Get();

			RenderBufferLoadAction colorLoadAction = (clearFlag & ClearFlag.Color) != ClearFlag.None ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
			RenderBufferLoadAction depthLoadAction = (clearFlag & ClearFlag.Depth) != ClearFlag.None ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;

			cmd.SetRenderTarget(GBufferIdentifiers, renderingData.curCameraDepthIdentifier);

			cmd.ClearRenderTarget(false, true, Color.clear);

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
				context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
			}
			context.ExecuteCommandBuffer(cmd);

			CommandBufferPool.Release(cmd);
		}

		public override void OnCameraEnd(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraEnd(context, ref renderingData);

			var cmd = CommandBufferPool.Get();
			cmd.ReleaseTemporaryRT(GBufferConstantBuffer.GBuffer0);
			cmd.ReleaseTemporaryRT(GBufferConstantBuffer.GBuffer1);
			cmd.ReleaseTemporaryRT(GBufferConstantBuffer.GBuffer2);
			cmd.ReleaseTemporaryRT(GBufferConstantBuffer.GBuffer3);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override bool IsMainCameraTargetPass()
		{
			return true;
		}
	}
}
