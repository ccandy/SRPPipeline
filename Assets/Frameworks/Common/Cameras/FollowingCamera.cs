using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Frameworks.Common
{
	[RequireComponent(typeof(Camera))]
	public class FollowingCamera : MonoBehaviour
	{
		Camera mCamera = null;

		public GameObject FollowObj = null;

		public Vector3 FixedFollowOffset = Vector3.one;

		public Vector3 MinFollowOffset = Vector3.one * 0.5f;

		public Vector3 Velocity = Vector3.zero;

		public float MaxFollowSpeed = 10000.0f;

		public bool IsSelfUpdate = true;

		void Awake()
		{
			mCamera = GetComponent<Camera>();
		}

		void Update()
		{
			if (!IsSelfUpdate)
				return;

			UpdatePosition(Time.deltaTime);
		}

		public void UpdatePosition(float deltaTime)
		{
			if (FollowObj == null)
				return;

			transform.LookAt(FollowObj.transform);

			Vector3 distance = transform.position - FollowObj.transform.position;

			Vector3 dir = distance.normalized;

			float length = distance.magnitude;

			float fixedLength = FixedFollowOffset.magnitude;

			float minLength = MinFollowOffset.magnitude;

			Velocity = (fixedLength - length) * dir;

			transform.position += Velocity * deltaTime;
		}
	}
}
