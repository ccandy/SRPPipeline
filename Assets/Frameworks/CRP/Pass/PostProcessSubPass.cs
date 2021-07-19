using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public abstract class PostProcessSubPass
	{
		public abstract void Init();

		public abstract void Execute(RenderTargetIdentifier srcTarget, RenderTargetIdentifier dstTarget, ScriptableRenderContext context, RenderingData renderingData);
	}
}