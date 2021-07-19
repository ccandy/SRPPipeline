using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common.GameCollider
{
	public class Sphere : ColliderBase
	{
		public override type GetColliderType ()
		{
			return type.Sphere;
		}

		public Vector3 Center = Vector3.zero;

		public float Radius = 1.0f;

		public Vector3 worldCenter
		{
			get
			{
				return transform.TransformPoint(Center);
			}
		}

		new public Bounds bounds
		{
			get
			{
				return new Bounds(worldCenter, Vector3.one * Radius * 2.0f);
			}
		}


	}
}
