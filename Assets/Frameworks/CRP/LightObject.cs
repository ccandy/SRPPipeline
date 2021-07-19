using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.CRP
{
	public struct LightObjectData
	{
		public enum LightType
		{
			Directional = 0,
			Point = 1,
		}

		public LightType lightType;
		public Vector3	 Position;
		public Vector3	 Direction;
		public float	 Radius;
	}

	public class LightObject : MonoBehaviour
	{

	}
}
