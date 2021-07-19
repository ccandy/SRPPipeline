using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class DefaultRenderPassBlock : RenderPassBlock
	{
		public override void InitPasses()
		{
			ActivedRenderPasses.Add(new MainLightShadowCasterPass(this));
			ActivedRenderPasses.Add(new RenderObjectPass(this,"CustomForward", (int)RenderPassQueue.ForwardOpaque, true));
			ActivedRenderPasses.Add(new RenderObjectPass(this,"CustomForward", (int)RenderPassQueue.Transparent, false));
		}
	}
}
