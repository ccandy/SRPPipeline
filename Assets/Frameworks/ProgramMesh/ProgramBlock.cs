using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks.ProgramMesh
{
	public class ProgramBlock : ProgramMeshBase
	{
		public enum SplitMode
		{
			vertical	,
			horizontal	,
		}

		public SplitMode Mode = SplitMode.vertical;

		public Texture2D texture;

		public int PixelPerUnit = 100;

		public List<float> uvStep = new List<float>();

		void Awake()
		{
				
		}

		public override Mesh GenerateMesh()
		{
			if (texture == null)
			{
				return null;
			}

			Mesh mesh = new Mesh();

			uvStep.RemoveAll(v => v >= 1.0f || v <= 0.0f);

			int verticesNum = (uvStep.Count > 2 ? 2 : uvStep.Count + 2) * 2;

			Vector3[] vertices	= new Vector3[verticesNum];
			Vector2[] uvs		= new Vector2[verticesNum];

			int[] indices		= new int[ (uvStep.Count + 1)*6 ];

			float unitPerPixel = 1.0f / PixelPerUnit;

			Rect origenRect = new Rect(-0.5f * texture.width * unitPerPixel, -0.5f * texture.height * unitPerPixel, texture.width * unitPerPixel, texture.height * unitPerPixel);

			int index = 0;

			switch (Mode)
			{
				case SplitMode.vertical:
					{
						float topZ		= uvStep.Count > 0 ? uvStep[0] * unitPerPixel * texture.height : 0.0f;
						float bottomZ	= uvStep.Count > 1 ? (1.0f - uvStep[1]) * unitPerPixel * texture.height : 0.0f;

						float maxY = (1.0f - (topZ + bottomZ) / (unitPerPixel * texture.height)) * unitPerPixel * 0.5f * texture.height;
						float minY = -maxY;

						if (topZ != 0.0f)
						{
							vertices[index] = new Vector3(origenRect.min.x, maxY, topZ);
							uvs[index++] = new Vector2(0.0f, 1.0f);

							vertices[index] = new Vector3(origenRect.max.x, maxY, topZ);
							uvs[index++] = new Vector2(1.0f, 1.0f);
						}

						vertices[index] = new Vector3(origenRect.min.x, maxY, 0.0f);
						uvs[index++]	= new Vector2(0.0f, topZ != 0.0f? (1.0f - uvStep[0]) : 1.0f);

						vertices[index] = new Vector3(origenRect.max.x, maxY, 0.0f);
						uvs[index++] = new Vector2(1.0f, topZ != 0.0f ? (1.0f - uvStep[0]) : 1.0f);

						vertices[index] = new Vector3(origenRect.min.x, minY, 0.0f);
						uvs[index++] = new Vector2(0.0f, bottomZ != 0.0f ? (1.0f - uvStep[1]) : 0.0f);

						vertices[index] = new Vector3(origenRect.max.x, minY, 0.0f);
						uvs[index++] = new Vector2(1.0f, bottomZ != 0.0f ? (1.0f - uvStep[1]) : 0.0f);

						if (bottomZ != 0.0f)
						{
							vertices[index] = new Vector3(origenRect.min.x, minY, bottomZ);
							uvs[index++]	= new Vector2(0.0f, 0.0f);

							vertices[index] = new Vector3(origenRect.max.x, minY, bottomZ);
							uvs[index++]	= new Vector2(1.0f, 0.0f);
						}
					}
					break;
				case SplitMode.horizontal:
					{
						float leftZ = uvStep.Count > 0 ? uvStep[0] * unitPerPixel * texture.width : 0.0f;
						float rightZ = uvStep.Count > 1 ? (1.0f - uvStep[1]) * unitPerPixel * texture.width : 0.0f;

						float maxX = (1.0f - (leftZ + rightZ) / (unitPerPixel * texture.width)) * unitPerPixel * 0.5f * texture.width;
						float minX = -maxX;

						if (leftZ != 0.0f)
						{
							vertices[index] = new Vector3(minX, origenRect.min.y, leftZ);
							uvs[index++] = new Vector2(0.0f, 0.0f);

							vertices[index] = new Vector3(minX, origenRect.max.y, leftZ);
							uvs[index++] = new Vector2(0.0f, 1.0f);
						}

						vertices[index] = new Vector3( minX, origenRect.min.y, 0.0f);
						uvs[index++] = new Vector2( leftZ != 0.0f ? uvStep[0] : 0.0f, 0.0f);

						vertices[index] = new Vector3( minX, origenRect.max.y, 0.0f);
						uvs[index++] = new Vector2( leftZ != 0.0f ? uvStep[0] : 0.0f, 1.0f);

						vertices[index] = new Vector3( maxX, origenRect.min.y, 0.0f);
						uvs[index++] = new Vector2( rightZ != 0.0f ? uvStep[1] : 1.0f, 0.0f);

						vertices[index] = new Vector3( maxX, origenRect.max.y, 0.0f);
						uvs[index++] = new Vector2( rightZ != 0.0f ? uvStep[1] : 1.0f, 1.0f);

						if (rightZ != 0.0f)
						{
							vertices[index] = new Vector3( maxX, origenRect.min.y, rightZ);
							uvs[index++] = new Vector2(1.0f, 0.0f);

							vertices[index] = new Vector3( maxX, origenRect.max.y, rightZ);
							uvs[index++] = new Vector2(1.0f, 1.0f);
						}
					}
					break;
			}

			index = 0;
			for (int i = 0; index < indices.Length; i += 2)
			{
				indices[index++] = i;
				indices[index++] = i + 1;
				indices[index++] = i + 2;
				indices[index++] = i + 1;
				indices[index++] = i + 3;
				indices[index++] = i + 2;
			}

			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			return mesh;
		}
	}
}
