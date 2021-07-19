using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assets.Editor.Tools
{
	public class FindReferences : EditorWindow
	{
		static private string GetRelativeAssetsPath(string path)
		{
			return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
		}

		class ReferencesInformation
		{
			public string path = "";
			public string guid = "";
			public Object obj = null;
		}

		static List<ReferencesInformation> ReferenceTable = new List<ReferencesInformation>();

		private Vector2 m_ScrollPosition;

		[MenuItem("Assets/Find References", false, 10)]
		static private void Find()
		{
			ReferenceTable.Clear();

			EditorSettings.serializationMode = SerializationMode.ForceText;
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (!string.IsNullOrEmpty(path))
			{
				string guid = AssetDatabase.AssetPathToGUID(path);
				List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
				string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
					.Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
				int startIndex = 0;

				EditorApplication.update = delegate ()
				{
					string file = files[startIndex];

					bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

					if (Regex.IsMatch(File.ReadAllText(file), guid))
					{
						var relativeAssetsPath = GetRelativeAssetsPath(file);

						ReferencesInformation info = new ReferencesInformation();
						info.path = relativeAssetsPath;
						info.guid = guid;
						info.obj = AssetDatabase.LoadAssetAtPath<Object>(relativeAssetsPath);

						ReferenceTable.Add(info);
					}

					startIndex++;
					if (isCancel || startIndex >= files.Length)
					{
						EditorUtility.ClearProgressBar();
						EditorApplication.update = null;
						startIndex = 0;
						Debug.Log("匹配结束");

						OpenFindReferencesUI();
					}

				};
			}
		}

		public static FindReferences Instance;
		static void OpenFindReferencesUI()
		{
			Instance = EditorWindow.GetWindow<FindReferences>();
			Instance.Show();
			//Instance.ReadPathFile();
			Instance.titleContent.text = "FindReferences";
			Instance.minSize = new Vector2(600, 300);
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginVertical();

			m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

			for (int i = 0; i < ReferenceTable.Count; ++i)
			{
				var info = ReferenceTable[i];

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.ObjectField( info.path, info.obj, typeof(Object), true );

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();
		}
	}
}
