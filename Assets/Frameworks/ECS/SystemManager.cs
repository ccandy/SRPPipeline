using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frameworks.Common;

namespace Frameworks.ECS
{
	public partial class SystemManager : SingletonMonoBehaviour<SystemManager>
	{
		List<ISystem> m_SystemList = new List<ISystem>();

		public float GameFrameTime = 1.0f / 60.0f;

		float passTime = 0.0f;

		public void ClearSystem()
		{
			m_SystemList.Clear();
		}

		public void AddSystem(ISystem sys)
		{
			if (sys != null)
			{
				m_SystemList.Add(sys);
			}
		}

		// Update is called once per frame
		void Update()
		{
			float deltaTime = Time.deltaTime;

			passTime += deltaTime;
			while (passTime >= GameFrameTime)
			{
				for (int i = 0; i < m_SystemList.Count; ++i)
				{
					var system = m_SystemList[i];
					system.DoGameUpdate(GameFrameTime);
				}
				passTime -= GameFrameTime;
			}

			for (int i = 0; i < m_SystemList.Count; ++i)
			{
				var system = m_SystemList[i];
				system.DoRenderUpdate(deltaTime);
			}
		}

		void LateUpdate()
		{
			float deltaTime = Time.deltaTime;
			for (int i = 0; i < m_SystemList.Count; ++i)
			{
				var system = m_SystemList[i];
				system.DoLateUpdate(deltaTime);
			}
		}
	}
}

