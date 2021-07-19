using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Frameworks.CSV
{
	public class CSVMakerFactoryUI : EditorWindow
	{
		string csvPath;
		string csPath;

		string csvName;

		string CSFileName;

		bool IsUseKey;

		int KeyNum;

		bool IsBuildResourceCreateFunction;
		bool IsBuildMemoryCreateFunction;
		bool IsBuildNewFunction;

		public static CSVMakerFactoryUI Instance;
		[MenuItem("Tools/OldCSVTools/CSVMakerFactory/Open Maker Config")]
		static void OpenCSVMakerFactoryUI()
		{
			Instance = EditorWindow.GetWindow<CSVMakerFactoryUI>();
			Instance.Show();
			//Instance.ReadPathFile();
			Instance.titleContent.text = "BuildCSFile";
			Instance.minSize = new Vector2(600, 300);
		}

		[MenuItem("Tools/OldCSVTools/Rebuild All CSV Config")]
		static void CSVRebuildAll()
		{
			string csvBuilderConfigPath = Application.dataPath + "/Frameworks/ConfigReader/CSV/editor/CSVBuilder.csv";
			Byte[] curFileData = null;

			try
			{
				curFileData = File.ReadAllBytes(csvBuilderConfigPath);
			}
			catch (Exception)
			{
				Debug.LogError("CSV Config file not found, using Open Maker Config to build the single csv file first.");
				return;
			}

			CSVFile curCSVBuilderFile = null;

			curCSVBuilderFile = CSVFile.CreateCSVFileFromMemory(curFileData);
			CSVBuilderReader curCSVBuilder = CSVBuilderReader.Create(curCSVBuilderFile);

			Dictionary<string, CSVBuilderData>.Enumerator Iter = curCSVBuilder.DataInFile.GetEnumerator();

			while (Iter.MoveNext())
			{
				CSVBuilderData cur = Iter.Current.Value;

				CSVFile curCSVFile = CSVFile.CreateCSVFileByStream(cur.CSVPath);
				if (curCSVFile == null)
					continue;

				if (CSVMakerFactory.BuildCSFileData(curCSVFile, cur.IsUseKey, cur.KeyNum, cur.IsBuildResourceCreateFunction, cur.IsBuildMemoryCreateFunction, cur.IsBuildNewFunction, cur.CSPath))
					CSVMakerFactory.CreateCSFile(cur.CSPath);
			}


		}

		private bool SelectFile(ref string filePath, string Title, string extName)
		{
			//		string[] directories = Directory.GetDirectories("Assets", "Assets", SearchOption.AllDirectories);
			// 		string startFolder = "";
			// 		if (directories.Length > 0)
			// 		{
			// 			startFolder = directories[0];
			// 		}
			string path = "";

			path = EditorUtility.OpenFilePanel(Title, Application.dataPath, extName);

			if (path != "")
			{
				filePath = path;
				return true;
			}

			return false;
		}

		private bool SelectFolder(ref string folderPath)
		{
			string[] directories = Directory.GetDirectories("Assets", "Assets", SearchOption.AllDirectories);
			//string startFolder = "";
			if (directories.Length > 0)
			{
				//startFolder = directories[0];
			}
			string path = EditorUtility.OpenFolderPanel("CS Path", "", "");
			if (path != "")
			{
				folderPath = path;
				return true;
			}

			return false;
		}


		void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("CSV路径：", GUILayout.Width(60));
			EditorGUILayout.TextField(csvPath);
			if (GUILayout.Button("选择CSV路径", GUILayout.Width(100)))
			{
				if (SelectFile(ref csvPath, "选择CSV路径", "csv"))
				{
					csvName = csvPath.Substring(csvPath.LastIndexOfAny(new char[] { '\\', '/' }) + 1);
					CSFileName = csvName.Split(new string[] { "." }, StringSplitOptions.None)[0];
					CSFileName += ".cs";
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("CS路径：", GUILayout.Width(60));
			EditorGUILayout.TextField(csPath);
			if (GUILayout.Button("选择CS路径", GUILayout.Width(100)))
			{
				string csFolderPath = "";
				if (SelectFolder(ref csFolderPath))
				{
					if (CSFileName != "")
					{
						csPath = csFolderPath + "/" + CSFileName;
					}
					else
					{
						csPath = csFolderPath;
					}

				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("是否使用主Key：", GUILayout.Width(240));
			//		GUIStyle leftArrowStyle = GUI.skin.GetStyle("toggle");
			//		string[] label = { "" };
			IsUseKey = GUILayout.Toggle(IsUseKey, "");

			EditorGUILayout.EndHorizontal();

			if (IsUseKey)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Key列", GUILayout.Width(240));
				KeyNum = EditorGUILayout.IntField(KeyNum);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("是否创建 通过Resource文件夹创建资源函数：", GUILayout.Width(240));
			GUI.skin.GetStyle("toggle");
			IsBuildResourceCreateFunction = GUILayout.Toggle(IsBuildResourceCreateFunction, "");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("是否创建 通过byte数组创建资源函数：", GUILayout.Width(240));
			GUI.skin.GetStyle("toggle");
			IsBuildMemoryCreateFunction = GUILayout.Toggle(IsBuildMemoryCreateFunction, "");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("是否创建 新建文件对象函数：", GUILayout.Width(240));
			GUI.skin.GetStyle("toggle");
			IsBuildNewFunction = GUILayout.Toggle(IsBuildNewFunction, "");
			EditorGUILayout.EndHorizontal();
			

			if (GUILayout.Button("构建读取CSV的CS代码", GUILayout.Width(500)))
			{
				CSVFile curCSVFile = CSVFile.CreateCSVFileByStream(csvPath);
				if (curCSVFile == null)
				{
					return;
				}

				if (CSVMakerFactory.BuildCSFileData(curCSVFile, IsUseKey, KeyNum, IsBuildResourceCreateFunction, IsBuildMemoryCreateFunction, IsBuildNewFunction, csPath))
					CSVMakerFactory.CreateCSFile(csPath);
				string csvBuilderConfigPath = Application.dataPath + "/Frameworks/Common/ConfigReader/CSV/editor/CSVBuilder.csv";
				Byte[] curFileData = null;

				try
				{
					curFileData = File.ReadAllBytes(csvBuilderConfigPath);
				}
				catch (Exception)
				{
					curFileData = null;
				}

				CSVFile curCSVBuilderFile = null;

				if (curFileData == null)
				{
					curCSVBuilderFile = CSVFile.CreateCSVFileFromMemory(CSVBuilderReader.defaultCSVFileData);
				}
				else
				{
					curCSVBuilderFile = CSVFile.CreateCSVFileFromMemory(curFileData);
				}

				CSVBuilderReader curCSVBuilder = CSVBuilderReader.Create(curCSVBuilderFile);
				CSVBuilderData data = null;

				if (curCSVBuilder.DataInFile == null)
				{
					curCSVBuilder.DataInFile = new Dictionary<string, CSVBuilderData>();
				}

				if (!curCSVBuilder.DataInFile.TryGetValue(csvName, out data))
				{
					data = new CSVBuilderData();
					data.CSVName = csvName;
					data.CSPath = csPath;
					data.CSVPath = csvPath;
					data.IsUseKey = IsUseKey;
					data.KeyNum = KeyNum;
					data.IsBuildResourceCreateFunction = IsBuildResourceCreateFunction;
					data.IsBuildMemoryCreateFunction = IsBuildMemoryCreateFunction;
					curCSVBuilder.DataInFile.Add(csvName, data);
				}
				else
				{
					data.CSVName = csvName;
					data.CSPath = csPath;
					data.CSVPath = csvPath;
					data.IsUseKey = IsUseKey;
					data.KeyNum = KeyNum;
					data.IsBuildResourceCreateFunction = IsBuildResourceCreateFunction;
					data.IsBuildMemoryCreateFunction = IsBuildMemoryCreateFunction;
				}

				curCSVBuilder.SaveToCSV(csvBuilderConfigPath);

				AssetDatabase.Refresh();
			}

			EditorGUILayout.EndVertical();


		}
	}

}