using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.ProgramMesh
{
	public class ProgramMeshBase : MonoBehaviour
	{
		public MeshFilter bindMeshFilter = null;

		public string SavePath = "";

		public virtual void OnEnable()
		{
			DoGenerate();
		}

		public void DoGenerate()
		{
			if (bindMeshFilter == null)
			{
				bindMeshFilter = GetComponent<MeshFilter>();
				if (bindMeshFilter == null)
					return;
			}

			bindMeshFilter.sharedMesh = GenerateMesh();
		}

		public virtual Mesh GenerateMesh()
		{
			return null;
		}
	}
}