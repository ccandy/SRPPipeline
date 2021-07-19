using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulNodeTrail : MonoBehaviour
{
	[System.Serializable]
	public class ControlNodeData
	{
		public Vector3 CreateOffset		= Vector3.zero;
		public Vector3 Velocity			= Vector3.zero;
		public Vector3 Accelerate		= Vector3.zero;
	}

	[System.Serializable]
	public class NodeData
	{
		public Vector3	Position		= Vector3.zero;
		public Vector3	Velocity		= Vector3.zero;
		public Vector3	Accelerate		= Vector3.zero;
	}

	[System.Serializable]
	public class FrameData
	{
		public float			PassTime;
		public List<NodeData>	FrameNodeData = new List<NodeData>();
	}

	Mesh			m_TrailMesh			= null;

	List<Vector3>	m_Vertices;
	List<Vector2>	m_UVs;
	List<Color>		m_Colors;
	List<int>		m_Indices;

	int				m_ControlNodeNum	= 2;
	public int		m_FrameNum			= 15;

	public bool		IsLocalPhysicData	= true;

	public bool		IsCalNormal			= true;
	public float	FPS					= 60.0f;
	float			m_LastFramePassTime = 10000.0f;

	public List<ControlNodeData> m_ControlNodes;

	List<FrameData> m_FrameNodes;

	public MeshFilter m_MeshFilter;

	int BeginIndex = 0;

	void InitMesh()
	{
		if (m_TrailMesh == null)
		{
			m_TrailMesh = new Mesh();
		}

		m_TrailMesh.Clear();

		m_ControlNodeNum = m_ControlNodes == null ? 0 : m_ControlNodes.Count;

		int verticesNum = m_ControlNodeNum * m_FrameNum;
		int indicesNum	= 6 * m_ControlNodeNum * m_FrameNum;

		m_FrameNodes = new List<FrameData>(m_FrameNum);

		m_Vertices	= new List<Vector3>(verticesNum);

		m_UVs		= new List<Vector2>(verticesNum);
		m_Colors	= new List<Color>(verticesNum);

		for (int i = 0; i < verticesNum; ++i)
		{
			m_Vertices.Add(Vector3.zero);
			m_UVs.Add(Vector2.zero);
			m_Colors.Add(Color.white);
		}

		m_Indices	= new List<int>(indicesNum);

		for (int nodeIndex = 0; nodeIndex < m_ControlNodeNum - 1; ++nodeIndex)
		{

			for (int frameNum = 0; frameNum < m_FrameNum - 1; ++frameNum)
			{
				int i0 = (m_FrameNum * nodeIndex + frameNum);
				int i1 = (m_FrameNum * nodeIndex + frameNum + 1);
				int i2 = (m_FrameNum * (nodeIndex + 1) + frameNum);
				int i3 = (m_FrameNum * (nodeIndex + 1) + frameNum + 1);

				m_Indices.Add(i0);
				m_Indices.Add(i1);
				m_Indices.Add(i2);
				m_Indices.Add(i1);
				m_Indices.Add(i3);
				m_Indices.Add(i2);
			}
		}


		m_TrailMesh.SetVertices(m_Vertices);
		m_TrailMesh.SetUVs(0,m_UVs);
		m_TrailMesh.SetColors(m_Colors);
		m_TrailMesh.SetTriangles(m_Indices, 0);

		if (IsCalNormal)
			m_TrailMesh.RecalculateNormals();

		m_TrailMesh.RecalculateBounds();

	}

	void OnEnable()
	{
		InitMesh();
		if (m_MeshFilter != null)
		{
			m_MeshFilter.sharedMesh = m_TrailMesh;
		}
	}

	void Update()
	{
		UpdateFrameData();
		BuildCurFrameData();

		UpdateMesh();
	}

	void BuildCurFrameData()
	{
		m_LastFramePassTime += Time.deltaTime;

		float TimeInv = FPS >= float.Epsilon ? float.Epsilon : 1.0f / FPS;

		if (m_LastFramePassTime < TimeInv)
			return;

		if (BeginIndex >= m_FrameNodes.Count)
			m_FrameNodes.Add(new FrameData());

		var curFrame = m_FrameNodes[BeginIndex];

		curFrame.PassTime = 0.0f;

		if (curFrame.FrameNodeData.Count != m_ControlNodeNum)
		{
			curFrame.FrameNodeData.Clear();
			for (int i = 0; i < m_ControlNodeNum; ++i)
				curFrame.FrameNodeData.Add(new NodeData());
		}

		Vector3 curTransformPos = transform.localPosition;

		if (IsLocalPhysicData)
		{
			for (int i = 0; i < m_ControlNodeNum; ++i)
			{
				curFrame.FrameNodeData[i].Position		= transform.TransformPoint(m_ControlNodes[i].CreateOffset + curTransformPos);
				curFrame.FrameNodeData[i].Velocity		= transform.TransformVector(m_ControlNodes[i].Velocity);
				curFrame.FrameNodeData[i].Accelerate	= transform.TransformVector(m_ControlNodes[i].Accelerate);
			}
		}
		else
		{
			for (int i = 0; i < m_ControlNodeNum; ++i)
			{
				curFrame.FrameNodeData[i].Position		= transform.TransformPoint(m_ControlNodes[i].CreateOffset + curTransformPos);
				curFrame.FrameNodeData[i].Velocity		= m_ControlNodes[i].Velocity;
				curFrame.FrameNodeData[i].Accelerate	= m_ControlNodes[i].Accelerate;
			}
		}


		++BeginIndex;
		BeginIndex %= m_FrameNum;

		m_LastFramePassTime = Mathf.Repeat(m_LastFramePassTime, TimeInv);
	}

	void UpdateFrameData()
	{
		float deltaTime = Time.deltaTime;
		for (int frameIndex = 0; frameIndex < m_FrameNodes.Count; ++frameIndex)
		{
			var frameData = m_FrameNodes[frameIndex];

			frameData.PassTime += deltaTime;

			for (int i = 0; i < frameData.FrameNodeData.Count; ++i)
			{
				var nodeData = frameData.FrameNodeData[i];

				nodeData.Position += deltaTime * nodeData.Velocity + 0.5f * nodeData.Accelerate * deltaTime * deltaTime;
				nodeData.Velocity += nodeData.Accelerate * deltaTime;
			}
		}
	}

	#region Temp Data reduce GC
	Vector3 position = Vector3.zero;
	Vector2 CurPer = Vector2.zero;
	#endregion

	void UpdateMesh()
	{
		for (int frame = 0; frame < m_FrameNum; ++frame)
		{
			var curFrame = frame < m_FrameNodes.Count ? m_FrameNodes[(BeginIndex + frame) % m_FrameNodes.Count] : m_FrameNodes[m_FrameNodes.Count - 1];

			for (int nodeIndex = 0; nodeIndex < m_ControlNodeNum; ++nodeIndex)
			{
				int i = (m_FrameNum * nodeIndex + frame);

				CurPer.x	= (float)	frame			/ (float)(m_FrameNodes.Count - 1);
				CurPer.y	= (float)	nodeIndex		/ (float)(m_ControlNodeNum - 1);
				

				m_Vertices[i] = curFrame.FrameNodeData[nodeIndex].Position;

				m_UVs[i] = CurPer;

				m_Colors[i] = Color.white;

			}
		}

		m_TrailMesh.SetVertices(m_Vertices);
		m_TrailMesh.SetUVs(0, m_UVs);
		m_TrailMesh.SetColors(m_Colors);
		
		if (IsCalNormal)
			m_TrailMesh.RecalculateNormals();

		m_TrailMesh.RecalculateBounds();

		
	}
}
