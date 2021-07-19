using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common
{
	public class TransformAnimation : CurveAdapt
	{
		public bool IsUpdatePosition = false;
		public Vector3		startPosition = Vector3.zero;
		public Vector3		endPosition = Vector3.zero;

		public bool IsUpdateRotation = false;
		public Vector3		startEulerAngle = Vector3.zero;
		public Vector3		endEulerAngle = Vector3.zero;

		public bool IsUpdateScale = false;
		public Vector3		startScale	  = Vector3.one;
		public Vector3		endScale	= Vector3.one;

		public override void DoUpdate(float CurValue)
		{
			if (IsUpdatePosition)
			{
				transform.localPosition = Vector3.Lerp(startPosition, endPosition, CurValue);
			}

			if (IsUpdateRotation)
			{
				Vector3 eulerAngle = Vector3.Lerp(startEulerAngle, endEulerAngle, CurValue);

				transform.localRotation = Quaternion.Euler(eulerAngle);
			}

			if (IsUpdateScale)
			{
				transform.localScale = Vector3.Lerp(startScale, endScale, CurValue);
			}
		}
	}
}
