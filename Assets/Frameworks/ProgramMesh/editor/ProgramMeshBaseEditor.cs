using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Frameworks.ProgramMesh
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ProgramMeshBase), true)]
	public class ProgramMeshBaseEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Do Generate"))
			{
				for (int i = 0; i < targets.Length; ++i)
				{
					var mesh = targets[i] as ProgramMeshBase;
					if (mesh == null)
						continue;

					mesh.DoGenerate();
				}
			}

			if (GUILayout.Button("Save Mesh"))
			{
				for (int i = 0; i < targets.Length; ++i)
				{
					var mesh = targets[i] as ProgramMeshBase;
					if (mesh == null)
						continue;

					if ( string.IsNullOrEmpty(mesh.SavePath))
					{
						EditorFilePath filePath = new EditorFilePath();
						filePath.UISettingPath(mesh.gameObject.name, "asset", Application.dataPath, true, mesh.gameObject.name);
						mesh.SavePath = filePath.assetPath;
						if (string.IsNullOrEmpty(mesh.SavePath))
							continue;
					}

					AssetDatabase.CreateAsset( mesh.GenerateMesh(), mesh.SavePath);
				}

				AssetDatabase.Refresh();
			}
		}
	}
}