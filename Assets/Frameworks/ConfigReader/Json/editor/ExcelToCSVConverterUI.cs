using UnityEngine;
using UnityEditor;
using System.IO;

namespace Frameworks.Json
{
	public class ExcelToCSVConverterUI : EditorWindow
	{
		static ExcelToCSVConverterUI Instance = null;

		string csvFolderPath; 

		string excelFolderPath;

		[MenuItem("Tools/JsonTools/ExcelToCSVConverter")]
		static void ConvertExcelToCSV()
		{
			Instance = EditorWindow.GetWindow<ExcelToCSVConverterUI>();
			Instance.Show();

			Instance.titleContent.text = "ExcelToCSVConverter";
			
			Instance.minSize = new Vector2(600, 140);

			Instance.csvFolderPath = Application.dataPath + "/../Document/CSVConfig";
			Instance.excelFolderPath = Application.dataPath + "/../Document/ExcelConfig";
		}

		/// <summary>
		/// 运行cmd命令
		/// 会显示命令窗口
		/// </summary>
		/// <param name="cmdExe">指定应用程序的完整路径</param>
		/// <param name="cmdStr">执行命令行参数</param>
		static bool RunCmd(string cmdExe, string cmdStr)
		{
			bool result = false;
			try
			{
				using (System.Diagnostics.Process myPro = new System.Diagnostics.Process())
				{
					//指定启动进程是调用的应用程序和命令行参数
					System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(cmdExe, cmdStr);
					myPro.StartInfo = psi;
					myPro.Start();
					myPro.WaitForExit();
					result = true;
				}
			}
			catch
			{

			}
			return result;
		}

		void OnGUI()
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Excel目录：", GUILayout.Width(60));
			EditorGUILayout.TextField(excelFolderPath);
			if (GUILayout.Button("选择Excel目录", GUILayout.Width(100)))
			{
				if (EditorFileAPI.SelectFolder(ref excelFolderPath, "Excel目录"))
				{

				}
			}
			EditorGUILayout.EndHorizontal();

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

			if (GUILayout.Button("转换", GUILayout.Width(500)))
			{
				string pyScriptPath = Application.dataPath + "/../ExternTools/excel2Csv.py";

				var excelStrPathes = Directory.GetFiles(excelFolderPath, "*.xlsx");

				for (int i = 0; i < excelStrPathes.Length; ++i)
				{
					string csvPath = string.Format("{0}/{1}.csv", csvFolderPath, Path.GetFileNameWithoutExtension(excelStrPathes[i]));

					ExcelToCSVConverter.Convert(excelStrPathes[i], csvPath);

					//string cmd = string.Format(" {0} {1} {2}", pyScriptPath, excelStrPathes[i], csvPath);

					//RunCmd("python", cmd);
				}
			}

			EditorGUILayout.EndVertical();
		}
	}
}
