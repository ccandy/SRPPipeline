using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Frameworks.UIControl
{
	[RequireComponent(typeof(Canvas))]
	public class SceneUIRoot : MonoBehaviour
	{
		public static SceneUIRoot Instance
		{
			get
			{
				return ms_Instance;
			}
		}

		static SceneUIRoot ms_Instance = null;

		Canvas m_Canvas = null;

		void Awake()
		{
			m_Canvas = GetComponent<Canvas>();
		}
	}
}
