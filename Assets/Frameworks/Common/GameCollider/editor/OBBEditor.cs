using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Frameworks.Common.GameCollider
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(OBB), true)]
	class OBBEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
		}

		public void OnSceneGUI()
		{
			OBB obb = target as OBB;

			Bounds bounds = obb.bounds;

			// Draw OBB Self
			Handles.color = Color.red;
			Handles.matrix = obb.transform.localToWorldMatrix;

			//Vector3 worldCenter = obb.worldCenter;
			//Vector3 worldExtent = obb.worldExtent;

			Handles.DrawWireCube(obb.Center, obb.Size);

			// Draw Bound
			Handles.matrix = Matrix4x4.identity;
			Handles.color = Color.green;
			Handles.DrawWireCube(bounds.center, bounds.size);
			//Handles.DrawWireCube(obb.transform.TransformPoint(bounds.center), bounds.extents);
		}
	}
}
