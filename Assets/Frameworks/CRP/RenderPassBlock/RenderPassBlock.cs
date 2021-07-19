using Frameworks.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	/// <summary>
	/// Render Pass Block Class.
	/// When the pipeline render a camera, it will render the passes which the camera needed.
	/// Different camera might have different pass array. so use this class to make different render pass styles.
	/// 
	/// the  pass rendering might follow the rule:
	/// first render the pass that isn't use Camera view.
	/// then render the pass with camera view.
	/// 
	/// </summary>
	public abstract class RenderPassBlock : IDisposable
	{
		protected List<RenderPass> ActivedRenderPasses = new List<RenderPass>();

		public RenderPassBlock()
		{

		}

		public abstract void InitPasses();

		public virtual void AddPass(RenderPass pass)
		{
			if (pass == null)
			{
				Log.Print(LogLevel.Error, "Add Pass Error! Add a null pass");
				return;
			}
			ActivedRenderPasses.Add(pass);
		}


		Comparison<RenderPass> passComparison = (pass1, pass2) => { return pass1.PassQueueID - pass1.PassQueueID; };

		public void SortPasses()
		{
			ActivedRenderPasses.Sort(passComparison);
		}

		public virtual void ClearPasses()
		{
			for (int i = 0; i < ActivedRenderPasses.Count; ++i)
			{
				var pass = ActivedRenderPasses[i];
				if (pass == null)
					continue;

				pass.Dispose();
			}

			ActivedRenderPasses.Clear();
		}

		public virtual void Dispose()
		{
			ClearPasses();
		}

		public virtual void OnFrameBegin()
		{
			for (int i = 0; i < ActivedRenderPasses.Count; ++i)
			{
				var pass = ActivedRenderPasses[i];
				pass.OnFrameBegin();
			}
		}

		public virtual void OnFrameEnd()
		{
			for (int i = 0; i < ActivedRenderPasses.Count; ++i)
			{
				var pass = ActivedRenderPasses[i];
				pass.OnFrameEnd();
			}
		}

		public virtual void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			for (int i = 0; i < ActivedRenderPasses.Count; ++i)
			{
				var pass = ActivedRenderPasses[i];
				pass.OnCameraBegin(context, ref renderingData);
			}
		}

		public virtual void OnCameraEnd(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			for (int i = 0; i < ActivedRenderPasses.Count; ++i)
			{
				var pass = ActivedRenderPasses[i];
				pass.OnCameraEnd(context, ref renderingData);
			}
		}

		public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			bool hasRenderToCameraPass = false;

			ClearFlag clearFlag = ClearFlag.None;

			for (int i = 0; i < ActivedRenderPasses.Count; ++i)
			{
				var pass = ActivedRenderPasses[i];

				bool isMainCameraTargetPass = pass.IsMainCameraTargetPass();

				if (isMainCameraTargetPass && !hasRenderToCameraPass)
				{
					context.SetupCameraProperties(renderingData.cameraData.camera);
					clearFlag = !renderingData.cameraData.isOverlay ? ClearFlag.All : 
						(renderingData.cameraData.isClearDepth ? ClearFlag.Depth : ClearFlag.None);
				}
				else if (isMainCameraTargetPass && hasRenderToCameraPass)
				{
					clearFlag = ClearFlag.None;
				}
				else if (!isMainCameraTargetPass)
				{
					clearFlag = !renderingData.cameraData.isOverlay ? ClearFlag.All : 
						(renderingData.cameraData.isClearDepth ? ClearFlag.Depth : ClearFlag.None);
					hasRenderToCameraPass = false;
				}
				 
				pass.SetupTarget(context, ref renderingData, clearFlag);
				pass.Execute(context, ref renderingData);

				hasRenderToCameraPass |= isMainCameraTargetPass;
			}
		}
	}
}
