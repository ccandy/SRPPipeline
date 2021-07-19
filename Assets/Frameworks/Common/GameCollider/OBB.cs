using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common.GameCollider
{
	public class OBB : ColliderBase
	{
		public override type GetColliderType ()
		{
			return type.OBB;
		}

		public Vector3 Center = Vector3.zero;
		public Vector3 Size = Vector3.one;

		public Vector3 worldCenter
		{
			get
			{
				return transform.TransformPoint(Center);
			}
		}

		public Vector3 worldSize
		{
			get
			{
				Vector3 worldSize = Size;
				worldSize.x *= transform.lossyScale.x;
				worldSize.y *= transform.lossyScale.y;
				worldSize.z *= transform.lossyScale.z;

//				Transform trans = transform;
//
//				do
//				{
//					worldSize.x *= trans.localScale.x;
//					worldSize.y *= trans.localScale.y;
//					worldSize.z *= trans.localScale.z;
//
//					trans = trans.parent;
//				} while (trans != null);

				return worldSize;
			}
		}

		Vector3[] ObbPointTemp = new Vector3[8];

		new public Bounds bounds
		{
			get
			{
				Bounds output = new Bounds(Center, Size);

				ObbPointTemp[0] = output.min;

				ObbPointTemp[1] = output.min;
				ObbPointTemp[1].x = output.max.x;

				ObbPointTemp[2] = output.min;
				ObbPointTemp[2].y = output.max.y;
				
				ObbPointTemp[3] = output.min;
				ObbPointTemp[3].z = output.max.z;

				ObbPointTemp[4] = output.min;
				ObbPointTemp[4].y = output.max.y;
				ObbPointTemp[4].z = output.max.z;

				ObbPointTemp[5] = output.min;
				ObbPointTemp[5].x = output.max.x;
				ObbPointTemp[5].z = output.max.z;

				ObbPointTemp[6] = output.min;
				ObbPointTemp[6].x = output.max.x;
				ObbPointTemp[6].y = output.max.y;

				ObbPointTemp[7] = output.max;

				for (int i = 0; i < 8; ++i )
				{
					ObbPointTemp[i] = transform.TransformVector(ObbPointTemp[i]);
				}

				Vector3 min = ObbPointTemp[0];
				Vector3 max = ObbPointTemp[0];

				for (int i = 1; i < 8; ++i)
				{
					if (min.x > ObbPointTemp[i].x)
					{
						min.x = ObbPointTemp[i].x;
					}

					if (min.y > ObbPointTemp[i].y)
					{
						min.y = ObbPointTemp[i].y;
					}

					if (min.z > ObbPointTemp[i].z)
					{
						min.z = ObbPointTemp[i].z;
					}

					if (max.x < ObbPointTemp[i].x)
					{
						max.x = ObbPointTemp[i].x;
					}

					if (max.y < ObbPointTemp[i].y)
					{
						max.y = ObbPointTemp[i].y;
					}

					if (max.z < ObbPointTemp[i].z)
					{
						max.z = ObbPointTemp[i].z;
					}
				}

				output.center = transform.position + ((min + max) * 0.5f);

				output.size = max - min;

				return output;
			}
		}

		/*
#region test function params
#if USING_THREAD_UNSAFE
		// To save GC for test function
		static Vector3 box0WorldCenter;
		static Vector3 box0WorldExtent;
		
		static Vector3 box1WorldCenter;
		static Vector3 box1WorldExtent;

		static Vector3 v;

		static Vector3[] VA = new Vector3[3];
		static Vector3[] VB = new Vector3[3];

		static float[,] R = new float[3, 3];
		static float[,] FR = new float[3, 3];
		static float ra, rb, t;
#else

		// To save GC for test function
		Vector3 box0WorldCenter;
		Vector3 box0WorldExtent;
		
		Vector3 box1WorldCenter;
		Vector3 box1WorldExtent;

		Vector3 v;

		Vector3[] VA = new Vector3[3];
		Vector3[] VB = new Vector3[3];

		float[,] R = new float[3, 3];
		float[,] FR = new float[3, 3];
		float ra, rb, t;
#endif
#endregion

		public bool TestIntersects(Sphere sphere)
		{
			float distance = GetDistanceToWorldPoint(sphere.worldCenter);

			return distance <= sphere.Radius;
		}

		public float GetDistanceToWorldPoint(Vector3 pos)
		{
			Vector3 posInOBBLocalTrans = transform.InverseTransformPoint(pos);

			float min = Mathf.Abs(posInOBBLocalTrans[0]);

			for (int i = 1; i < 2; ++i)
			{
				float cur = Mathf.Abs(posInOBBLocalTrans[i]);

				if (cur < min)
					min = cur;
			}

			return min;
		}

		// Using: http://blog.csdn.net/silangquan/article/details/50812425
		// Separating Axis Test, this need to check the axis plane 15 times.
		public bool TestIntersects(OBB box1)
		{
			box0WorldCenter = worldCenter;
			box0WorldExtent = worldSize * 0.5f;

			box1WorldCenter = box1.worldCenter;
			box1WorldExtent = box1.worldSize * 0.5f;

			v = box1WorldCenter - box0WorldCenter;

			VA[0] = transform.rotation * Vector3.right;
			VA[1] = transform.rotation * Vector3.up;
			VA[2] = transform.rotation * Vector3.forward;

			VB[0] = box1.transform.rotation * Vector3.right;
			VB[1] = box1.transform.rotation * Vector3.up;
			VB[2] = box1.transform.rotation * Vector3.forward;

			Vector3 T = new Vector3(Vector3.Dot(v, VA[0]), Vector3.Dot(v, VA[1]), Vector3.Dot(v, VA[2]));

			for (int i = 0; i < 3; ++i)
			{
				for (int k = 0; k < 3; ++k)
				{
					R[i, k] = Vector3.Dot(VA[i], VB[k]);
					FR[i, k] = float.Epsilon + Mathf.Abs(R[i, k]);
				}
			}

			// A's basis vectors  
			for (int i = 0; i < 3; ++i)
			{
				ra = box0WorldExtent[i];
				rb = box1WorldExtent[0] * FR[i, 0] + box1WorldExtent[1] * FR[i, 1] + box1WorldExtent[2] * FR[i, 2];
				t = Mathf.Abs(T[i]);
				if (t > ra + rb) 
					return false;
			}

			// B's basis vectors  
			for (int k = 0; k < 3; ++k)
			{
				ra = box0WorldExtent[0] * FR[0, k] + box0WorldExtent[1] * FR[1, k] + box0WorldExtent[2] * FR[2, k];
				rb = box1WorldExtent[k];
				t = Mathf.Abs(T[0] * R[0, k] + T[1] * R[1, k] + T[2] * R[2, k]);
				if (t > ra + rb) 
					return false;
			}

			//9 cross products  

			//L = A0 x B0  
			ra = box0WorldExtent[1] * FR[2, 0] + box0WorldExtent[2] * FR[1, 0];
			rb = box1WorldExtent[1] * FR[0, 2] + box1WorldExtent[2] * FR[0, 1];
			t = Mathf.Abs(T[2] * R[1, 0] - T[1] * R[2, 0]);
			if (t > ra + rb) 
				return false;

			//L = A0 x B1  
			ra = box0WorldExtent[1] * FR[2, 1] + box0WorldExtent[2] * FR[1, 1];
			rb = box1WorldExtent[0] * FR[0, 2] + box1WorldExtent[2] * FR[0, 0];
			t = Mathf.Abs(T[2] * R[1, 1] - T[1] * R[2, 1]);
			if (t > ra + rb) 
				return false;

			//L = A0 x B2  
			ra = box0WorldExtent[1] * FR[2, 2] + box0WorldExtent[2] * FR[1, 2];
			rb = box1WorldExtent[0] * FR[0, 1] + box1WorldExtent[1] * FR[0, 0];
			t = Mathf.Abs(T[2] * R[1, 2] - T[1] * R[2, 2]);
			if (t > ra + rb) 
				return false;

			//L = A1 x B0  
			ra = box0WorldExtent[0] * FR[2, 0] + box0WorldExtent[2] * FR[0, 0];
			rb = box1WorldExtent[1] * FR[1, 2] + box1WorldExtent[2] * FR[1, 1];
			t = Mathf.Abs(T[0] * R[2, 0] - T[2] * R[0, 0]);
			if (t > ra + rb) 
				return false;

			//L = A1 x B1  
			ra = box0WorldExtent[0] * FR[2, 1] + box0WorldExtent[2] * FR[0, 1];
			rb = box1WorldExtent[0] * FR[1, 2] + box1WorldExtent[2] * FR[1, 0];
			t = Mathf.Abs(T[0] * R[2, 1] - T[2] * R[0, 1]);
			if (t > ra + rb) 
				return false;

			//L = A1 x B2  
			ra = box0WorldExtent[0] * FR[2, 2] + box0WorldExtent[2] * FR[0, 2];
			rb = box1WorldExtent[0] * FR[1, 1] + box1WorldExtent[1] * FR[1, 0];
			t = Mathf.Abs(T[0] * R[2, 2] - T[2] * R[0, 2]);
			if (t > ra + rb) 
				return false;

			//L = A2 x B0  
			ra = box0WorldExtent[0] * FR[1, 0] + box0WorldExtent[1] * FR[0, 0];
			rb = box1WorldExtent[1] * FR[2, 2] + box1WorldExtent[2] * FR[2, 1];
			t = Mathf.Abs(T[1] * R[0, 0] - T[0] * R[1, 0]);
			if (t > ra + rb) 
				return false;

			//L = A2 x B1  
			ra = box0WorldExtent[0] * FR[1, 1] + box0WorldExtent[1] * FR[0, 1];
			rb = box1WorldExtent[0] * FR[2, 2] + box1WorldExtent[2] * FR[2, 0];
			t = Mathf.Abs(T[1] * R[0, 1] - T[0] * R[1, 1]);
			if (t > ra + rb) 
				return false;

			//L = A2 x B2  
			ra = box0WorldExtent[0] * FR[1, 2] + box0WorldExtent[1] * FR[0, 2];
			rb = box1WorldExtent[0] * FR[2, 1] + box1WorldExtent[1] * FR[2, 0];
			t = Mathf.Abs(T[1] * R[0, 2] - T[0] * R[1, 2]);
			if (t > ra + rb) 
				return false;
 
			return true;
		}

		// Remove the 9 cross of 3D intersect. This will make about 15% mistake.
		// But it work faster than TestIntersects function.
		public bool TestIntersectsSimple(OBB box1)
		{
			box0WorldCenter = worldCenter;
			box0WorldExtent = worldSize * 0.5f;

			box1WorldCenter = box1.worldCenter;
			box1WorldExtent = box1.worldSize * 0.5f;

			v = box1WorldCenter - box0WorldCenter;

			VA[0] = transform.rotation * Vector3.right;
			VA[1] = transform.rotation * Vector3.up;
			VA[2] = transform.rotation * Vector3.forward;

			VB[0] = box1.transform.rotation * Vector3.right;
			VB[1] = box1.transform.rotation * Vector3.up;
			VB[2] = box1.transform.rotation * Vector3.forward;

			Vector3 T = new Vector3(Vector3.Dot(v, VA[0]), Vector3.Dot(v, VA[1]), Vector3.Dot(v, VA[2]));

			for (int i = 0; i < 3; ++i)
			{
				for (int k = 0; k < 3; ++k)
				{
					R[i, k] = Vector3.Dot(VA[i], VB[k]);
					FR[i, k] = float.Epsilon + Mathf.Abs(R[i, k]);
				}
			}

			// A's basis vectors  
			for (int i = 0; i < 3; ++i)
			{
				ra = box0WorldExtent[i];
				rb = box1WorldExtent[0] * FR[i, 0] + box1WorldExtent[1] * FR[i, 1] + box1WorldExtent[2] * FR[i, 2];
				t = Mathf.Abs(T[i]);
				if (t > ra + rb)
					return false;
			}

			// B's basis vectors  
			for (int k = 0; k < 3; ++k)
			{
				ra = box0WorldExtent[0] * FR[0, k] + box0WorldExtent[1] * FR[1, k] + box0WorldExtent[2] * FR[2, k];
				rb = box1WorldExtent[k];
				t = Mathf.Abs(T[0] * R[0, k] + T[1] * R[1, k] + T[2] * R[2, k]);
				if (t > ra + rb)
					return false;
			}

			return true;
		}

		// This alway used when only rotator Z Axis.
		public bool TestIntersects2D(OBB box1)
		{
			box0WorldCenter = worldCenter;
			box0WorldExtent = worldSize * 0.5f;

			box1WorldCenter = box1.worldCenter;
			box1WorldExtent = box1.worldSize * 0.5f;

			v = box1WorldCenter - box0WorldCenter;

			VA[0] = transform.rotation * Vector3.right;
			VA[1] = transform.rotation * Vector3.up;

			VB[0] = box1.transform.rotation * Vector3.right;
			VB[1] = box1.transform.rotation * Vector3.up;

			Vector3 T = new Vector3(Vector3.Dot(v, VA[0]), Vector3.Dot(v, VA[1]), 0.0f);

			for (int i = 0; i < 2; ++i)
			{
				for (int k = 0; k < 2; ++k)
				{
					R[i, k] = Vector3.Dot(VA[i], VB[k]);
					FR[i, k] = float.Epsilon + Mathf.Abs(R[i, k]);
				}
			}

			// A's basis vectors  
			for (int i = 0; i < 2; ++i)
			{
				ra = box0WorldExtent[i];
				rb = box1WorldExtent[0] * FR[i, 0] + box1WorldExtent[1] * FR[i, 1];
				t = Mathf.Abs(T[i]);
				if (t > ra + rb)
					return false;
			}

			// B's basis vectors  
			for (int k = 0; k < 2; ++k)
			{
				ra = box0WorldExtent[0] * FR[0, k] + box0WorldExtent[1] * FR[1, k];
				rb = box1WorldExtent[k];
				t = Mathf.Abs(T[0] * R[0, k] + T[1] * R[1, k] + T[2] * R[2, k]);
				if (t > ra + rb)
					return false;
			}

// 			//4 cross products  
// 
// 			//L = A0 x B0  
// 			ra = box0WorldExtent[1] * FR[2, 0] + box0WorldExtent[2] * FR[1, 0];
// 			rb = box1WorldExtent[1] * FR[0, 2] + box1WorldExtent[2] * FR[0, 1];
// 			t = Mathf.Abs(T[2] * R[1, 0] - T[1] * R[2, 0]);
// 			if (t > ra + rb)
// 				return false;
// 
// 			//L = A0 x B1  
// 			ra = box0WorldExtent[1] * FR[2, 1] + box0WorldExtent[2] * FR[1, 1];
// 			rb = box1WorldExtent[0] * FR[0, 2] + box1WorldExtent[2] * FR[0, 0];
// 			t = Mathf.Abs(T[2] * R[1, 1] - T[1] * R[2, 1]);
// 			if (t > ra + rb)
// 				return false;
// 
// 			//L = A1 x B0  
// 			ra = box0WorldExtent[0] * FR[2, 0] + box0WorldExtent[2] * FR[0, 0];
// 			rb = box1WorldExtent[1] * FR[1, 2] + box1WorldExtent[2] * FR[1, 1];
// 			t = Mathf.Abs(T[0] * R[2, 0] - T[2] * R[0, 0]);
// 			if (t > ra + rb)
// 				return false;
// 
// 			//L = A1 x B1  
// 			ra = box0WorldExtent[0] * FR[2, 1] + box0WorldExtent[2] * FR[0, 1];
// 			rb = box1WorldExtent[0] * FR[1, 2] + box1WorldExtent[2] * FR[1, 0];
// 			t = Mathf.Abs(T[0] * R[2, 1] - T[2] * R[0, 1]);
// 			if (t > ra + rb)
// 				return false;

			return true;
		}
	    */
	}
}
