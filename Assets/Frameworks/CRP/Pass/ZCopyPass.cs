using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class ZCopyPass : RenderPass
	{
		Material ZCopyMaterial = new Material(Shader.Find("Hidden/CustomRenderPipeline/ZCopy"));

		public static class ConstantBuffer
		{
			public static int _SrcDepth		= Shader.PropertyToID("_SrcDepth");
			public static int _DepthTexture = Shader.PropertyToID("_DepthTexture");
		}

		RenderTargetIdentifier m_DepthTarget;

		public ZCopyPass(RenderPassBlock bindBlock, int renderPassQueue = (int)RenderPassQueue.EarlyZ + 1) : base(bindBlock, " ZCopy", renderPassQueue)
		{
			profilingSampler = new ProfilingSampler(nameof(ZCopyPass));
			filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		}

		public override void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraBegin(context, ref renderingData);

			CommandBuffer cmd = CommandBufferPool.Get();

			m_DepthTarget = new RenderTargetIdentifier(ConstantBuffer._DepthTexture, 0, CubemapFace.Unknown);

			var descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.colorFormat = RenderTextureFormat.Depth;
			descriptor.depthBufferBits = 32; //TODO: do we really need this. double check;
			descriptor.msaaSamples = 1;

			cmd.GetTemporaryRT(ConstantBuffer._DepthTexture, descriptor, FilterMode.Point);
			//cmd.GetTemporaryRT(ConstantBuffer._DepthTexture
			//	, renderingData.cameraData.cameraTargetDescriptor.width
			//	, renderingData.cameraData.cameraTargetDescriptor.height
			//	, 32
			//	, FilterMode.Point
			//	, RenderTextureFormat.RFloat);

			context.ExecuteCommandBuffer(cmd);

			CommandBufferPool.Release(cmd);
		}

		public override void SetupTarget(ScriptableRenderContext context, ref RenderingData renderingData, ClearFlag clearFlag)
		{
			var cmd = CommandBufferPool.Get();
			//using var scope = new ProfilingScope(cmd, profilingSampler);
			//RenderBaseFunction.SetRenderTarget(cmd, ref m_DepthTarget, ref m_DepthTarget, ClearFlag.All, renderingData.cameraData.clearColor);
			//cmd.SetRenderTarget(m_DepthTarget, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
			cmd.SetRenderTarget(m_DepthTarget, m_DepthTarget, 0, CubemapFace.Unknown, -1);
			cmd.ClearRenderTarget(false, true, Color.clear);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, profilingSampler))
			{
				//RenderBaseFunction.Blit(cmd, renderingData.curCameraDepthIdentifier, m_DepthTarget, ZCopyMaterial);
				cmd.SetGlobalTexture(ConstantBuffer._SrcDepth, renderingData.curCameraDepthIdentifier);
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();

				//cmd.Blit(renderingData.curCameraDepthIdentifier, m_DepthTarget);
				//cmd.Blit( renderingData.curCameraDepthIdentifier, BuiltinRenderTextureType.CurrentActive, ZCopyMaterial, 0);
				cmd.DrawMesh(RenderBaseFunction.fullscreenMesh, Matrix4x4.identity, ZCopyMaterial, 0, 0);
				//context.ExecuteCommandBuffer(cmd);
				//cmd.Clear();
				//cmd.SetGlobalTexture(ConstantBuffer._DepthTexture, m_DepthTarget, RenderTextureSubElement.Depth);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void OnCameraEnd(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraEnd(context, ref renderingData);

			var cmd = CommandBufferPool.Get();

			cmd.ReleaseTemporaryRT(ConstantBuffer._DepthTexture);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override bool IsMainCameraTargetPass()
		{
			return true;
		}

		
	}
}