using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public enum RenderPassQueue
	{
		MainLightShadowMap	= 100,
		EarlyZ				= 200,
		GBuffer				= 300,
		DeferredLighting	= 400,
		ForwardOpaque		= 500,
		SkyBox				= 600,
		Transparent			= 700,
		FinalPass			= 800,
	}


	public abstract partial class RenderPass : IDisposable
	{
		public RenderTargetIdentifier[] ColorAttachments;
		public RenderTargetIdentifier DepthAttachments;

		protected ProfilingSampler profilingSampler;

		protected RenderPassBlock m_BindBlock;

		public RenderPassBlock bindBlock
		{
			get => m_BindBlock;
		}

		public bool OverrideCameraTarget = false;

		public int PassQueueID = (int)RenderPassQueue.ForwardOpaque;

		public int layermask = -1;

		public StencilState stencilState = StencilState.defaultValue;

		public FilteringSettings filteringSettings;

		public Material overrideMaterial = null;

		public ShaderTagId PassShaderTag
		{
			get => m_PassShaderTag;
		}

		ShaderTagId m_PassShaderTag;

		protected List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

		public string PassName
		{
			get => m_PassName;
		}

		string m_PassName;

		public RenderPass(RenderPassBlock bindBlock, string passName, int queueID)
		{
			m_BindBlock = bindBlock;
			m_PassName = passName;
			m_PassShaderTag = new ShaderTagId(m_PassName);
			PassQueueID = queueID;

			ColorAttachments = new RenderTargetIdentifier[] { BuiltinRenderTextureType.CameraTarget, 0, 0, 0, 0, 0, 0, 0 };
			DepthAttachments = BuiltinRenderTextureType.CameraTarget;

		}

		public abstract bool IsMainCameraTargetPass();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="renderingData"></param>
		/// <param name="clearFlag"></param>
		/// <returns>is camera target</returns>
		public virtual void SetupTarget(ScriptableRenderContext context, ref RenderingData renderingData, ClearFlag clearFlag)
		{
			var cmd = CommandBufferPool.Get();
			using var scope = new ProfilingScope(cmd, profilingSampler);

			RenderBaseFunction.SetRenderTarget(cmd, ref renderingData.curCameraColorIdentifier, ref renderingData.curCameraDepthIdentifier, clearFlag, renderingData.cameraData.clearColor);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public abstract void Execute(ScriptableRenderContext context, ref RenderingData renderingData);

		public virtual void OnFrameBegin()
		{

		}

		public virtual void OnFrameEnd()
		{

		}

		public virtual void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{

		}

		public virtual void OnCameraEnd(ScriptableRenderContext context, ref RenderingData renderingData)
		{

		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
