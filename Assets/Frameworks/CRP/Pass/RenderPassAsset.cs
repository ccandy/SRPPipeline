using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace Frameworks.CRP
{
	public abstract class RenderPassAsset : ScriptableObject
	{
		public string	PassName;

		public int		Queue;

		public bool		isOpaque;

		public LayerMask layerMask = -1;

		public StencilState stencilState = StencilState.defaultValue;

		public Material overrideMaterial = null;

		public abstract RenderPass CreatePass(RenderPassBlock bindBlock);
	}
}
