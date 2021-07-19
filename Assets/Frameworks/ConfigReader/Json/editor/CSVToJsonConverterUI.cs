using UnityEngine;
using UnityEditor;
using System.IO;

namespace Frameworks.Json
{
	public class CSVToJsonConverterUI : EditorWindow
	{
		static CSVToJsonConverterUI Instance = null;

		int convertTypevalue = 1;

		string csvPath;

		string jsonPath;

		string csvFolderPath;

		string jsonFolderPath;

		[MenuItem("Tools/JsonTools/CSVToJsonConverter")]
		static void OpenCSVToJsonConverter()
		{

			Instance = EditorWindow.GetWindow<CSVToJsonConverterUI>();
			Instance.Show();
			//Instance.ReadPathFile();
			Instance.titleContent.text = "CSVToJsonConverter";
			Instance.minSize = new Vector2(600, 140);

			Instance.csvFolderPath = Application.dataPath + "/../Document/CSVConfig";
			Instance.jsonFolderPath = Application.dataPath + "/BundleRes/Config";
		}



		void OnGUI()
		{
			int[] converTypeValues = new int[] { 0, 1 };
			string[] converTypeString = new string[] { "Convert Single File", "Convert File Folder" };

			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			convertTypevalue = EditorGUILayout.IntPopup("Convert Type:", convertTypevalue, converTypeString, converTypeValues);

			switch (convertTypevalue)
			{
				case 0: // single file
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("CSV路径：", GUILayout.Width(60));
						EditorGUILayout.TextField(csvPath);
						if (GUILayout.Button("选择CSV路径", GUILayout.Width(100)))
						{
							if (EditorFileAPI.SelectFile(ref csvPath, "选择CSV路径", "csv"))
							{
								jsonPath = csvPath;

								jsonPath = jsonPath.Replace(".csv", ".json");
							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.Space();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Json路径：", GUILayout.Width(60));
						EditorGUILayout.TextField(jsonPath);
						if (GUILayout.Button("选择Json路径", GUILayout.Width(100)))
						{
							EditorFileAPI.SelectFile(ref jsonPath, "选择Json路径", "json");
						}
						EditorGUILayout.EndHorizontal();

						if (GUILayout.Button("转换", GUILayout.Width(500)))
						{
							Frameworks.Json.JsonTools.ConvertCSVToJson(csvPath, jsonPath);
						}

						AssetDatabase.Refresh();
					}
					break;
				case 1: // folder
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("CSV目录：", GUILayout.Width(60));
						EditorGUILayout.TextField(csvFolderPath);
						if (GUILayout.Button("选择CSV目录", GUILayout.Width(100)))
						{
							if (EditorFileAPI.SelectFolder(ref csvFolderPath, "CSV目录"))
							{

							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.Space();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Json目录：", GUILayout.Width(60));
						EditorGUILayout.TextField(jsonFolderPath);
						if (GUILayout.Button("选择Json目录", GUILayout.Width(100)))
						{
							if (EditorFileAPI.SelectFolder(ref jsonFolderPath, "Json目录"))
							{

							}
						}
						EditorGUILayout.EndHorizontal();

						if (GUILayout.Button("转换", GUILayout.Width(500)))
						{
							var csvStrPathes = Directory.GetFiles(csvFolderPath, "*.csv");

							for (int i = 0; i < csvStrPathes.Length; ++i)
							{
								string jsonPath1 = string.Format("{0}/{1}.json", jsonFolderPath, Path.GetFileNameWithoutExtension(csvStrPathes[i]));

								Frameworks.Json.JsonTools.ConvertCSVToJson(csvStrPathes[i], jsonPath1);
							}
						}

						AssetDatabase.Refresh();
					}
					break;
			}

			EditorGUILayout.EndVertical();
		}
	}
}