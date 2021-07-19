using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frameworks
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(DyPlaneMesh))]
	public class DyPlaneMeshEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();

			var plane = (DyPlaneMesh)target;

			if (GUILayout.Button("Rebuild Mesh"))
			{
				plane.CreateMeshs();
			}
		}
	}
}
