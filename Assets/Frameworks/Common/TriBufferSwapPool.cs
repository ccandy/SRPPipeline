using System;


namespace Frameworks.Common
{
	public class TriBufferSwapPool<T> where T : new()
	{
		const int MUL_BUFFER_SIZE = 3;
		const int UNLOCK_INDEX = -1;

		T[] buffer =null;

		volatile int m_CurLockIndex = UNLOCK_INDEX; // Buffer index currently locked for reading
		volatile int m_CurWritingIndex = 2;         // Buffer index of currently writing
		volatile int m_CurMostPostIndex = 1;            // Buffer index of most currently Post

		object m_Indexlock = new object();

		public TriBufferSwapPool()
		{
			buffer = new T[MUL_BUFFER_SIZE];
			buffer[0] = new T();
			buffer[1] = new T();
			buffer[2] = new T();
		}

		/// <summary>
		///  Low-level methods:
		/// </summary>
		/// <param name="bufferIndex"></param>
		/// <returns></returns>
		public T getBuffer(int bufferIndex) // Low-level method to access triple buffer contents
		{
			return buffer[bufferIndex];
		}

		/// <summary>
		/// GetCurWritingValue. Use this function when begin to write value  
		/// </summary>
		/// <returns></returns>
		public T CurWritingValue
		{
			get
			{
				return buffer[m_CurWritingIndex];
			}

			set
			{
				buffer[m_CurWritingIndex] = value;
			}
		}

		/// <summary>
		/// SwapWriteValue. Use this function after write the value.
		/// </summary>
		/// <returns></returns>
		public T SwapWriteValue()
		{
			lock(m_Indexlock)
			{
				m_CurMostPostIndex = m_CurWritingIndex;

				++m_CurWritingIndex;
				m_CurWritingIndex %= MUL_BUFFER_SIZE;

				while (m_CurWritingIndex == m_CurLockIndex || m_CurWritingIndex == m_CurMostPostIndex)
				{
					++m_CurWritingIndex;
					m_CurWritingIndex %= MUL_BUFFER_SIZE;
				}
			}

			return buffer[m_CurWritingIndex];
		}



		public int GetCurWritingIndex()
		{
			return m_CurWritingIndex;
		}

		public int GetCurLockIndex()
		{
			return m_CurLockIndex;
		}

		/// <summary>
		/// LockReadValue  
		/// </summary>
		/// <returns></returns>
		public T LockReadValue()
		{
			lock (m_Indexlock)
			{
				if (m_CurLockIndex == UNLOCK_INDEX)
				{
					m_CurLockIndex = m_CurMostPostIndex;
				}
			}
			return buffer[m_CurLockIndex];
		}

		/// <summary>
		/// UnlockReadValue Call this after read the value.
		/// </summary>
		public void UnlockReadValue()
		{
			lock (m_Indexlock)
			{
				m_CurLockIndex = UNLOCK_INDEX;
			}
		}

		

	}
}
