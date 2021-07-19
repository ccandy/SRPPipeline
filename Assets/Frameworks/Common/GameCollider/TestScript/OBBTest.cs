using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common.GameCollider
{
	public class OBBTest : MonoBehaviour
	{
		public ColliderBase box0;
		public ColliderBase box1;

		MeshRenderer box0Render = null;

		// Use this for initialization
		void Start()
		{
			if (box0 == null || box1 == null)
				return;

			box0Render = box0.GetComponent<MeshRenderer>();

		}

		// Update is called once per frame
		void Update()
		{
			if (box0Render == null)
				return;

			if (ColliderBase.TestIntersects2D(box0,box1))
			{
				box0Render.material.color = Color.red;
			}
			else
			{
				box0Render.material.color = Color.green;
			}
		}
	}
}
