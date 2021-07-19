using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class DeferredRenderPassBlock : RenderPassBlock
	{
		public override void InitPasses()
		{
			ActivedRenderPasses.Add(new MainLightShadowCasterPass(this));
			ActivedRenderPasses.Add(new EarlyZPass(this));
			ActivedRenderPasses.Add(new ZCopyPass(this));
			ActivedRenderPasses.Add(new GBufferPass(this));
			ActivedRenderPasses.Add(new DeferredLightPass(this));
			ActivedRenderPasses.Add(new RenderObjectPass(this, "CustomForward", (int)RenderPassQueue.ForwardOpaque, true));
			ActivedRenderPasses.Add(new RenderObjectPass(this,"CustomForward", (int)RenderPassQueue.Transparent, false));
			ActivedRenderPasses.Add(new FinalPass(this));
		}

		public override void OnCameraBegin(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			RenderBaseFunction.CreateCameraRenderTarget(context, ref renderingData, ref renderingData.cameraData.cameraTargetDescriptor, true, true);
			base.OnCameraBegin(context, ref renderingData);
		}

		public override void OnCameraEnd(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			base.OnCameraEnd(context, ref renderingData);
			RenderBaseFunction.ReleaseCreatedCameraRenderTarget(context, ref renderingData);

		}
	}
}
