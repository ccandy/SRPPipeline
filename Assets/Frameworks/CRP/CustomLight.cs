using System;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Frameworks.CRP
{

	public class CustomLight : MonoBehaviour
	{
		enum LightType
		{
			Point,
			Capsule,
		}

		public Color color			= Color.white;
		public float intensity		= 1.0f;

		public float radius			= 1.0f;
	}
}