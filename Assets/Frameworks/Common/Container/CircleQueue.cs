using System;
using System.Collections.Generic;
using System.Text;
//using UnityEngine;

namespace Frameworks.Common
{
	/// <summary>
	/// Circle Queue. Malloc the size of the queue when init.when inert the data will set to the last point of the queue.
	/// When the queue is full, the new insert data will cover the head of the queue.
	/// 
	/// this queue will clear the data when resize it.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CircleQueue<T>
	{
		T[] m_Pool = null;

		int m_curPtrIndex = 0;
		int m_Count = 0;

		public void Init(int PoolNum = 1)
		{
			m_Pool = new T[PoolNum];
			m_curPtrIndex = 0;
		}

		public CircleQueue(int PoolNum = 1)
		{
			Init(PoolNum);
		}

		public void Enqueue(T data)
		{
			m_Pool[m_curPtrIndex] = data;

			m_curPtrIndex = (m_curPtrIndex + 1)%m_Pool.Length;

			if (m_Count < m_Pool.Length)
				++m_Count;	
		}

		public T Dequeue()
		{
			if (m_Count == 0)
				return default(T);

			int headIndex = (m_curPtrIndex - 1 + m_Pool.Length - m_Count) % m_Pool.Length;

			var rt = m_Pool[headIndex];
			--m_Count;
			m_Pool[headIndex] = default(T);

			return rt;
		}

		public T GetData(int index)
		{
			if (m_Count == 0)
				return default(T);

			int realIndex = (m_curPtrIndex - 1 + m_Pool.Length - m_Count + index) % m_Pool.Length;
			return m_Pool[realIndex];
		}

		public T GetLastData()
		{
			if (m_Count == 0)
				return default(T);

			int realIndex = (m_curPtrIndex - 1 + m_Pool.Length) % m_Pool.Length;

			return m_Pool[realIndex];
		}

		public void Clear()
		{
			for (int i = 0; i < m_Pool.Length; ++i)
			{
				m_Pool[i] = default(T);
			}

			m_curPtrIndex = 0;
		}
	}
}
