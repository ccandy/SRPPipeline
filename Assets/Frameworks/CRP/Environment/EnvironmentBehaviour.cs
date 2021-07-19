using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.CRP
{
	[Serializable]
	public struct EnvironmentBaseData
	{
		public bool isGlobal;
		public Bounds	FullPercentBounds;
		public float	ReduceDistance;

		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;
	}

	public class EnvironmentBehaviour : MonoBehaviour
	{
		

	}
}
