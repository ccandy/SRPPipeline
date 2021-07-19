using UnityEngine;
using UnityEditor;
using System.IO;

namespace Frameworks.Json
{
	public class JsonReaderBuilderUI : EditorWindow
	{
		const string BuilderHistoryDirectory = "../.BuilderHistory";

		static JsonReaderBuilderUI Instance = null;

		string csvPath = "";
		string csFolderPath = "";
		string csPath = "";
		string fileName = "";

		bool IsNeedBuildScalaCode = true;

		string scalaPackageName = "config";
		string scalaFolderPath = "";
		string scalaPath = "";

		[MenuItem("Tools/JsonTools/JsonReaderBuilder")]
		static void OpenJsonReaderBuilder()
		{
			Instance = EditorWindow.GetWindow<JsonReaderBuilderUI>();
			Instance.Show();
			//Instance.ReadPathFile();
			Instance.titleContent.text = "JsonReaderBuilder";
			Instance.minSize = new Vector2(600, 140);

			//Instance.csvPath = string.Format("{0}/{1}", Application.dataPath, "../Document/CSVConfig");

			Instance.csFolderPath = string.Format("{0}/{1}", Application.dataPath, "Scripts/GenJsonReader");
		}

		void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("CSV路径：", GUILayout.Width(60));
			csvPath = EditorGUILayout.TextField(csvPath);
			if (GUILayout.Button("选择CSV路径", GUILayout.Width(100)))
			{
				if (EditorFileAPI.SelectFile(ref csvPath, "选择CSV路径", "csv", string.Format("{0}/{1}", Application.dataPath, "../Document/CSVConfig")))
				{
					fileName = Path.GetFileNameWithoutExtension(csvPath);
					csPath		= string.Format("{0}/{1}.cs", csFolderPath, fileName);
					scalaPath	= string.Format("{0}/{1}.scala", scalaFolderPath, fileName);
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("CS路径：", GUILayout.Width(60));
			csPath = EditorGUILayout.TextField(csPath);
			if (GUILayout.Button("选择CS路径", GUILayout.Width(100)))
			{
				if (EditorFileAPI.SelectFolder(ref csFolderPath, "选择CS路径"))
				{
					if (fileName != "")
					{
						csPath = string.Format("{0}/{1}.cs", csFolderPath, fileName);
					}
					else
					{
						csPath = csFolderPath;
					}

				}
			}
			EditorGUILayout.EndHorizontal();

			IsNeedBuildScalaCode = EditorGUILayout.Toggle("是否需要生成Scala读取json代码", IsNeedBuildScalaCode);

			if (IsNeedBuildScalaCode)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Scala 包名：", GUILayout.Width(60));
				scalaPackageName = EditorGUILayout.TextField(scalaPackageName);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Scala路径：", GUILayout.Width(60));
				scalaPath = EditorGUILayout.TextField(scalaPath);
				if (GUILayout.Button("选择Scala路径", GUILayout.Width(100)))
				{
					if (EditorFileAPI.SelectFolder(ref scalaFolderPath, "选择Scala路径"))
					{
						if (fileName != "")
						{
							scalaPath = string.Format("{0}/{1}.scala", scalaFolderPath, fileName);
						}
						else
						{
							scalaPath = scalaFolderPath;
						}

					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if (GUILayout.Button("构建读取Json代码", GUILayout.Width(500)))
			{
				if (JsonTools.BuildJsonReaderFromCSV(csvPath, csPath))
				{
					JsonTools.SaveJsonReaderInfoToHistory(csvPath, csPath);
					AssetDatabase.Refresh();
				}

				if (IsNeedBuildScalaCode)
				{
					JsonTools.BuildScalaJsonReaderFromCSV(csvPath, scalaPath, scalaPackageName);
				}
			}

			EditorGUILayout.EndVertical();
		}
	}
}
