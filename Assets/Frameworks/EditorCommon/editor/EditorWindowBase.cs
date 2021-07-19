using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Frameworks
{
	public class EditorWindowBase<T> : EditorWindow where T : EditorWindow
	{
		public static T Instance;

		public static void Open(string title, Vector2 size)
		{
			Instance = EditorWindow.GetWindow<T>();
			Instance.Show();

			if (Instance.position.x < 0 || Instance.position.y < 0
				|| Instance.position.x > 1920 || Instance.position.y > 1080)
			{
				Instance.position = new Rect(0,0, size.x, size.y);
			}

			//Instance.ReadPathFile();
			Instance.titleContent.text = title;
			Instance.minSize = size;
		}
	}
}
