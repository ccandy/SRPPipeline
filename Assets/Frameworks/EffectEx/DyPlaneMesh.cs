using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class DyPlaneMesh : MonoBehaviour
	{
		MeshFilter		m_MeshFilter	= null;
		MeshRenderer	m_MeshRenderer	= null;

		public Vector2Int VertexCount	= Vector2Int.one;
		public Vector2    MeshSize		= Vector2.one;

		#region Mesh Data

		List<Vector3>	Position;
		List<Vector2>	UV0;
		List<Color>		Colores;

		List<int>		Indices;

		Mesh			m_Mesh			= null;

		#endregion

		void Awake()
		{
			m_MeshFilter				= GetComponent<MeshFilter>();
			m_MeshRenderer				= GetComponent<MeshRenderer>();
			m_Mesh						= new Mesh();

			m_MeshFilter.sharedMesh		= m_Mesh;
		}

		void Start()
		{
			CreateMeshs();
		}

		public void CreateMeshs()
		{
			Position		= new List<Vector3>((VertexCount.x + 1) * (VertexCount.y + 1));
			UV0				= new List<Vector2>((VertexCount.x + 1) * (VertexCount.y + 1));
			Colores			= new List<Color>((VertexCount.x + 1) * (VertexCount.y + 1));

			Indices			= new List<int>((VertexCount.x + 1) * (VertexCount.y + 1) * 6 / 4);

			float xDetla	= MeshSize.x / VertexCount.x;
			float zDetla	= MeshSize.y / VertexCount.y;

			float xBegin	= -0.5f * MeshSize.x;
			float zBegin	= 0.5f * MeshSize.y;

			for (int x = 0; x <= VertexCount.x; ++x)
			{
				for (int y = 0; y <= VertexCount.y; ++y)
				{
					Position.Add(Vector3.zero);
					UV0.Add(Vector2.zero);
					Colores.Add(Color.white);
				}
			}

			for (int x = 0; x <= VertexCount.x; ++x)
			{
				for (int y = 0; y <= VertexCount.y; ++y)
				{
					Position[(VertexCount.x + 1) * y + x]	= new Vector3(xBegin + xDetla * x, 0.0f, zBegin - zDetla * y);
					UV0[(VertexCount.x + 1) * y + x]		= new Vector2( ((float)x) / VertexCount.x, ((float)y) / VertexCount.y);
				}
			}

			for (int x = 0; x < VertexCount.x; ++x)
			{
				for (int y = 0; y < VertexCount.y; ++y)
				{
					int i0 = ( (VertexCount.x + 1) * y + x);
					int i1 = ( (VertexCount.x + 1) * y + x + 1);
					int i2 = ( (VertexCount.x + 1) * (y + 1) + x);
					int i3 = ( (VertexCount.x + 1) * (y + 1) + x + 1);

					Indices.Add(i0);
					Indices.Add(i1);
					Indices.Add(i2);
					Indices.Add(i1);
					Indices.Add(i3);
					Indices.Add(i2);
				}
			}

			m_Mesh.Clear();

			m_Mesh.SetVertices( Position);
			m_Mesh.SetUVs( 0, UV0);

			m_Mesh.SetTriangles(Indices, 0);
			m_Mesh.RecalculateNormals();
			m_Mesh.RecalculateBounds();
		}
	}
}
