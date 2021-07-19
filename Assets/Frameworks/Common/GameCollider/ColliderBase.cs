using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.Common.GameCollider
{
	public class ColliderBase : MonoBehaviour
	{
		public enum type
		{
			Undefined,
			OBB,
			Sphere,
			Capsule,
		}

		public virtual type GetColliderType ()
		{
			return type.Undefined;
		}

		public Bounds bounds
		{
			get
			{
			 	return new Bounds (transform.position, Vector3.zero);
			}
		}

		#region test function params
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
		#endregion
		 
		delegate bool testFunc(ColliderBase collider0,ColliderBase collider1);

		static testFunc[,] testFunc3D = new testFunc[,] 
		{
			{TestIntersects_OBB2OBB,TestIntersects_OBB2Sphere},
			{TestIntersects_Sphere2OBB,TestIntersects_Sphere2Sphere}
		};

		static testFunc[,] testFunc2D = new testFunc[,] 
		{
			{TestIntersects_OBB2OBB_2D,TestIntersects_OBB2Sphere},
			{TestIntersects_Sphere2OBB,TestIntersects_Sphere2Sphere}
		};


		public static bool TestIntersects(ColliderBase collider0,ColliderBase collider1)
		{
			return testFunc3D [(int)collider0.GetColliderType (), (int)collider1.GetColliderType ()] (collider0, collider1);
		}

		public static bool TestIntersects2D(ColliderBase collider0,ColliderBase collider1)
		{
			return testFunc2D [(int)collider0.GetColliderType (), (int)collider1.GetColliderType ()] (collider0, collider1);
		}

		public static float GetDistanceToWorldPoint( ColliderBase collider0, Vector3 pos)
		{
			OBB box = (OBB)collider0;

			Vector3 posOffset = pos - box.transform.position;

			Quaternion invRotation = Quaternion.Inverse (box.transform.rotation);

			posOffset = invRotation * posOffset;

			Vector3 NearestPoint = Vector3.zero;

			for (int i = 0; i < 2; ++i)
			{
				posOffset [i] = Mathf.Abs (posOffset [i]);
				if ( posOffset [i] < box.worldSize [i] * 0.5f)
					NearestPoint [i] = posOffset [i];
				else
					NearestPoint [i] = box.worldSize [i] * 0.5f;
			}

			return (posOffset - NearestPoint).magnitude;
		}

		public static bool TestIntersects_OBB2Sphere(ColliderBase collider0, ColliderBase collider1)
		{
			OBB box = (OBB)collider0;
			Sphere sphere = (Sphere)collider1;
			
			float distance = GetDistanceToWorldPoint( box, sphere.worldCenter);

			return distance <= sphere.Radius;
		}

		public static bool TestIntersects_Sphere2OBB(ColliderBase collider0, ColliderBase collider1)
		{
			Sphere sphere = (Sphere)collider0;
			OBB box = (OBB)collider1;

			float distance = GetDistanceToWorldPoint( box, sphere.worldCenter);

			return distance <= sphere.Radius;
		}

		public static bool TestIntersects_Sphere2Sphere(ColliderBase collider0, ColliderBase collider1)
		{
			Sphere sphere0 = (Sphere)collider0;
			Sphere sphere1 = (Sphere)collider1;

			return (sphere0.worldCenter - sphere1.worldCenter).sqrMagnitude 
				<= (sphere0.Radius + sphere1.Radius) * (sphere0.Radius + sphere1.Radius);
		}

		// Using: http://blog.csdn.net/silangquan/article/details/50812425
		// Separating Axis Test, this need to check the axis plane 15 times.
		public static bool TestIntersects_OBB2OBB(ColliderBase collider0, ColliderBase collider1)
		{
			OBB box0 = (OBB)collider0;
			OBB box1 = (OBB)collider1;

			box0WorldCenter = box0.worldCenter;
			box0WorldExtent = box0.worldSize * 0.5f;

			box1WorldCenter = box1.worldCenter;
			box1WorldExtent = box1.worldSize * 0.5f;

			v = box1WorldCenter - box0WorldCenter;

			VA[0] = box0.transform.rotation * Vector3.right;
			VA[1] = box0.transform.rotation * Vector3.up;
			VA[2] = box0.transform.rotation * Vector3.forward;

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
		public static bool TestIntersects_OBB2OBB_Simple(ColliderBase collider0, ColliderBase collider1)
		{
			OBB box0 = (OBB)collider0;
			OBB box1 = (OBB)collider1;

			box0WorldCenter = box0.worldCenter;
			box0WorldExtent = box0.worldSize * 0.5f;

			box1WorldCenter = box1.worldCenter;
			box1WorldExtent = box1.worldSize * 0.5f;

			v = box1WorldCenter - box0WorldCenter;

			VA[0] = box0.transform.rotation * Vector3.right;
			VA[1] = box0.transform.rotation * Vector3.up;
			VA[2] = box0.transform.rotation * Vector3.forward;

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
		public static bool TestIntersects_OBB2OBB_2D(ColliderBase collider0, ColliderBase collider1)
		{
			OBB box0 = (OBB)collider0;
			OBB box1 = (OBB)collider1;

			box0WorldCenter = box0.worldCenter;
			box0WorldExtent = box0.worldSize * 0.5f;

			box1WorldCenter = box1.worldCenter;
			box1WorldExtent = box1.worldSize * 0.5f;

			v = box1WorldCenter - box0WorldCenter;

			VA[0] = box0.transform.rotation * Vector3.right;
			VA[1] = box0.transform.rotation * Vector3.up;

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


	}
}
