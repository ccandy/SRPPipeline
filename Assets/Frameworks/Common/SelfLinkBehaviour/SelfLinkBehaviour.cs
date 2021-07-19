using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Frameworks.Common
{
	public class SelfLinkBehaviour<T> : MonoBehaviour where T : MonoBehaviour 
	{
		static SelfLinkBehaviour<T>			ms_Header = null;
		static SelfLinkBehaviour<T>			ms_Tailer = null;

		public static SelfLinkBehaviour<T>	Header
		{
			get
			{
				return ms_Header;
			}
		}

		public static SelfLinkBehaviour<T>	Tailer
		{
			get
			{
				return ms_Tailer;
			}
		}

		SelfLinkBehaviour<T>				m_Prev		= null;
		SelfLinkBehaviour<T>				m_Next		= null;
		bool								m_IsInList	= false;

		public SelfLinkBehaviour<T>			Prev
		{
			get
			{
				return m_Prev;
			}
		}

		public SelfLinkBehaviour<T>			Next
		{
			get
			{
				return m_Next;
			}
		}

		public bool							IsInList
		{
			get
			{
				return m_IsInList;
			}
		}

		public void Insert()
		{
			if (IsInList)
				return;

			if (ms_Header == null)
			{
				ms_Header = this;
			}

			if (ms_Tailer == null)
			{
				ms_Tailer = this;
			}
			else
			{
				ms_Tailer.m_Next = this;
				m_Prev = ms_Tailer;
			}
			
			ms_Tailer = this;

			m_IsInList = true;
		}

		public void Remove()
		{
			if (!IsInList)
				return;

			if (Prev != null)
			{
				Prev.m_Next = Next;
			}

			if (Next != null)
			{
				Next.m_Prev = Prev;
			}

			if (ms_Header == this)
			{
				ms_Header = m_Next;
			}

			if (ms_Tailer == this)
			{
				ms_Tailer = m_Prev;
			}

			m_Prev = null;
			m_Next = null;
			m_IsInList = false;
		}

	}
}