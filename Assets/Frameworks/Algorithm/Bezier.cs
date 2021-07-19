using System;
using UnityEngine;

namespace Frameworks.Algorithm
{
	public class Bezier : MonoBehaviour
	{
		public enum CurveType
		{
			OneControl,
			TwoControl,
		}

		public CurveType curveType = CurveType.OneControl;

		public Vector3 Begin;

		public Vector3 ControlPosition0;

		public Vector3 ControlPosition1;

		public Vector3 End;

		public LineRenderer ShowRender = null;

		public delegate Vector3 BezierFunc(float t);

		public float LineLength = 0.0f;

		Vector3[] tempPoints;

		void Update()
		{
			BezierFunc func = BezierT_2;
			if (curveType == CurveType.TwoControl)
				func = BezierT_3;

			OnUpdateRenderer(func);
			//LineLength = Length(Speractor, func);
		}

		void OnUpdateRenderer(BezierFunc func)
		{
			if (ShowRender == null)
				return;

			if (ShowRender.positionCount == 0)
				return;

			if (tempPoints == null || tempPoints.Length != ShowRender.positionCount)
				tempPoints = new Vector3[ShowRender.positionCount];

			if (func == null)
				return;

			float t = 0.0f;
		
			for (int i = 0; i < tempPoints.Length; ++i)
			{
				t = ((float)i) / (ShowRender.positionCount - 1);
				tempPoints[i] = func(t);
			}

			ShowRender.SetPositions(tempPoints);
		}

		public float CalLength(int spretatorNum, BezierFunc func)
		{
			float len = 0.0f;

			Vector3 lastPos = Begin;

			float delta = 1.0f / spretatorNum;

			if (delta < 0.000001f)
				delta = 0.000001f;

			float t = delta;

			Vector3 nextPos;

			for (; t < 1.0f; t += delta)
			{
				nextPos = func(t);
				len += Vector3.Distance(nextPos, lastPos);
				lastPos = nextPos;
			}

			return len;
		}

		public float CalLength(int spretatorNum)
		{
			BezierFunc func = BezierT_2;
			if (curveType == CurveType.TwoControl)
				func = BezierT_3;

			return CalLength(spretatorNum, func);
		}


		Vector3 BezierT_2(float t)
		{
			float oneMinusT = 1.0f - t;
			return Begin * (oneMinusT * oneMinusT) + ControlPosition0 * (t * oneMinusT * 2.0f) + End * (t * t);
		}

		Vector3 BezierT_3(float t)
		{
			float oneMinusT = 1.0f - t;
			return Begin * (oneMinusT * oneMinusT * oneMinusT) 
				+ (t * oneMinusT * oneMinusT * 3.0f) * ControlPosition0
				+ (t * t * oneMinusT * 3.0f) * ControlPosition1 
				+ End * (t * t * t) ;
		}
	}
}
