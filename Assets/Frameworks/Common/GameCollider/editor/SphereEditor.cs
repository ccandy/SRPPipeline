using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Frameworks.Common.GameCollider
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Sphere))]
	class SphereEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
		}

		public void OnSceneGUI()
		{
			Sphere sphere = target as Sphere;

			Bounds bounds = sphere.bounds;

			// Draw OBB Self
			Handles.color = Color.red;
			//Handles.matrix = sphere.transform.localToWorldMatrix;

			//Vector3 worldCenter = obb.worldCenter;
			//Vector3 worldExtent = obb.worldExtent;

			Handles.DrawWireDisc(sphere.worldCenter, Camera.current.transform.up, sphere.Radius);
			Handles.DrawWireDisc(sphere.worldCenter, Camera.current.transform.forward, sphere.Radius);
			Handles.DrawWireDisc(sphere.worldCenter, Camera.current.transform.right, sphere.Radius);

			//Handles.DrawWireCube(obb.Center, obb.Size);

			// Draw Bound
			Handles.matrix = Matrix4x4.identity;
			Handles.color = Color.green;
			Handles.DrawWireCube(bounds.center, bounds.size);
			//Handles.DrawWireCube(obb.transform.TransformPoint(bounds.center), bounds.extents);
		}
	}
}
