using UnityEngine;
using System.Collections;

namespace Frameworks.ECS
{
	public abstract class ISystem
	{
		public virtual void DoRenderUpdate(float deltaTime)
		{

		}

		public virtual void DoGameUpdate(float fixedDeltaTime)
		{

		}

		public virtual void DoLateUpdate(float deltaTime)
		{

		}
	}
}

