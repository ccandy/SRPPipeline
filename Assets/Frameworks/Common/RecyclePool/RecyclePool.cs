using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Frameworks.Common
{
	public class RecyclePool<T> where T : MonoBehaviour
	{
		List<T> m_RecycleObjs = new List<T>();

		public void ClearRecycleList()
		{
			if (m_RecycleObjs == null)
				return;

			m_RecycleObjs.Clear();
		}

		public void SetNewObjFunc(OnNewObjFunc func)
		{
			m_OnNewObj = func;
		}

		public void SetOnResetObjFunc(OnResetObj func)
		{
			m_OnResetObj = func;
		}

		public delegate T OnNewObjFunc();
		public delegate void OnResetObj(T obj);
		public delegate void OnRecycleObj(T obj);

		protected OnNewObjFunc m_OnNewObj = null;

		protected OnResetObj m_OnResetObj = null;

		public T DoCreateOrReuseObj()
		{
			T reuseObj = default(T);
			if (m_RecycleObjs == null)
			{
				m_RecycleObjs = new List<T>();

				if (m_OnNewObj == null)
				{
					return reuseObj;
				}

				reuseObj = m_OnNewObj();
				if (m_OnResetObj != null)
				{
					m_OnResetObj(reuseObj);
				}

				return reuseObj;
			}

			if (m_RecycleObjs.Count == 0)
			{
				if (m_OnNewObj == null)
				{
					return default(T);
				}

				reuseObj = m_OnNewObj();

				m_OnResetObj?.Invoke(reuseObj);

				return reuseObj;
			}

			int reuseIndex = m_RecycleObjs.Count - 1;

			reuseObj = m_RecycleObjs[reuseIndex];
			m_RecycleObjs.RemoveAt(reuseIndex);

			if (reuseObj == null)
			{
				reuseObj = m_OnNewObj();
				if (m_OnResetObj != null)
				{
					m_OnResetObj(reuseObj);
				}

				return reuseObj;
			}

			reuseObj.gameObject.SetActive(true);

			if (m_OnResetObj != null)
			{
				m_OnResetObj(reuseObj);
			}

			return reuseObj;
		}

		public void DoRecycle(T obj)
		{
			if (obj == null)
				return;

			if (m_RecycleObjs == null)
			{
				m_RecycleObjs = new List<T>();
			}

			obj.gameObject.SetActive(false);

			m_RecycleObjs.Add(obj);
		}
	}
}
