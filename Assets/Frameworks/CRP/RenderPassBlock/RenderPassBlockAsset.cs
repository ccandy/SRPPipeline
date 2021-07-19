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
	public abstract class RenderPassBlockAsset : ScriptableObject, ISerializationCallbackReceiver
	{
		public List<RenderPassAsset> CustomPassAssets = new List<RenderPassAsset>();

		public RenderPassBlockAsset()
		{
			
		}

		public abstract RenderPassBlock CreatePassBlock();

		public virtual void OnAfterDeserialize()
		{
			
		}

		public virtual void OnBeforeSerialize()
		{
			
		}
	}
}
