using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Frameworks.CRP
{
	public abstract class PostProcessSubPassAsset :  ScriptableObject
	{
		public abstract PostProcessSubPass Create();
	}
}