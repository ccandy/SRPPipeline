using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(LineRenderer))]
	public class TrailEx : MonoBehaviour
	{
		[Header("带子顶点生命时长(越大则越长)")]
		public float time = 5.0f;

		[Header("带子初始物理速度")]
		public Vector3 BaseVelocity = Vector3.zero;

		[Header("带子物理加速度")]
		public Vector3 AddVelocity = Vector3.zero;

		[Header("带子随机加速度最大偏移量")]
		public Vector3 RandomAddVelocityMax = Vector3.zero;

		float tick = 0;

		int mTotalPosition = 10;

		LineRenderer mLineRenderer = null;

		Vector3[] m_TrailPosition = null;
		Vector3[] m_TrailVelocity = null;

		Vector3[] mInputPosition = null;

		int BeginIndex = 0;

		private void Awake()
		{
			mLineRenderer = GetComponent<LineRenderer>();

			mTotalPosition = (int)(time * 60.0f) + 1;

			mLineRenderer.positionCount = mTotalPosition;
		}

		// Use this for initialization
		void Start()
		{
			ResetPoint();
		}

		void OnEnable()
		{
			ResetPoint();
		}

		void ResetPoint()
		{
			m_TrailPosition = new Vector3[mTotalPosition];
			m_TrailVelocity = new Vector3[mTotalPosition];
			mInputPosition = new Vector3[mTotalPosition];
			if (mLineRenderer.useWorldSpace)
			{
				for (int i = 0; i < mTotalPosition; ++i)
				{
					m_TrailPosition[i] = transform.position;
					mInputPosition[i] = transform.position;
					m_TrailVelocity[i] = BaseVelocity;
				}
			}
			else
			{
				for (int i = 0; i < mTotalPosition; ++i)
				{
					m_TrailPosition[i] = transform.localPosition;
					mInputPosition[i] = transform.localPosition;
					m_TrailVelocity[i] = BaseVelocity;
				}
			}

			BeginIndex = m_TrailPosition.Length - 1;
		}

		void UpdatePhysic()
		{
			Vector3 RandomAddVelocity = Vector3.zero;
			RandomAddVelocity.x = RandomAddVelocityMax.x * (Mathf.PerlinNoise(tick, 0) - 0.5f );
			RandomAddVelocity.y = RandomAddVelocityMax.y * (Mathf.PerlinNoise(0, tick) - 0.5f);
			RandomAddVelocity.z = RandomAddVelocityMax.z * (Mathf.PerlinNoise(tick, tick) - 0.5f);

			tick += Time.deltaTime;

			for (int i = 0; i < m_TrailPosition.Length; ++i)
			{
				if (BeginIndex == i)
					continue;

				m_TrailPosition[i] += m_TrailVelocity[i] * Time.deltaTime + 0.5f * (AddVelocity + RandomAddVelocity) * Time.deltaTime * Time.deltaTime;
				m_TrailVelocity[i] += (AddVelocity + RandomAddVelocity) * Time.deltaTime;
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (mTotalPosition != (int)(time * 60.0f) + 1)
			{
				mTotalPosition = (int)(time * 60.0f) + 1;
				mLineRenderer.positionCount = mTotalPosition;
				ResetPoint();
			}

			if (m_TrailPosition == null)
				return;

			#region renew the current position
			if (mLineRenderer.useWorldSpace)
			{
				m_TrailPosition[BeginIndex] = transform.position;
			}
			else
			{
				m_TrailPosition[BeginIndex] = transform.localPosition;
			}
			m_TrailVelocity[BeginIndex] = BaseVelocity;
			#endregion

			for (int i = 0; i < m_TrailPosition.Length; ++i)
			{
				int curIndex = (BeginIndex + i) % m_TrailPosition.Length;
				mInputPosition[i] = m_TrailPosition[curIndex];
			}

			UpdatePhysic();

			mLineRenderer.SetPositions(mInputPosition);

			BeginIndex += m_TrailPosition.Length - 1;
			BeginIndex %= m_TrailPosition.Length;
		}
	}
}
