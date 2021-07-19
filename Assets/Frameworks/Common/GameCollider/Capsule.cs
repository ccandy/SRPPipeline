using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common.GameCollider
{
	public class Capsule : ColliderBase
	{
		public override type GetColliderType()
		{
			return type.Capsule;
		}

		//
		// 摘要:
		//     The center of the capsule, measured in the object's local space.
		public Vector3 center = Vector3.zero;
		//
		// 摘要:
		//     The radius of the sphere, measured in the object's local space.
		public float radius = 1.0f;
		//
		// 摘要:
		//     The height of the capsule measured in the object's local space.
		public float height = 1.0f;
		//
		// 摘要:
		//     The direction of the capsule.
		public int direction = 0;
	}
}
