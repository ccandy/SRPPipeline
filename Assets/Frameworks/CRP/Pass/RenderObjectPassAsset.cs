using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public class RenderObjectPassAsset : RenderPassAsset
	{
		
		public override RenderPass CreatePass(RenderPassBlock bindBlock)
		{
			var pass = new RenderObjectPass( bindBlock, PassName, Queue, isOpaque, layerMask.value);
			pass.stencilState = stencilState;
			return pass;
		}
	}
}
