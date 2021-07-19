using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class PostProcessPass : RenderPass
	{
		public PostProcessPass(RenderPassBlock bindBlock) : base(bindBlock, "PostProcessPass", (int)RenderPassQueue.FinalPass)
		{
			profilingSampler = new ProfilingSampler(nameof(PostProcessPass));
			filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		}

		public override void SetupTarget(ScriptableRenderContext context, ref RenderingData renderingData, ClearFlag clearFlag)
		{
			// doing nothing post process pass is muli-pass. it swap target every pass.
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			
		}

		public override bool IsMainCameraTargetPass()
		{
			return true;
		}
	}
}