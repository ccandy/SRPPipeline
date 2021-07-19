using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace Frameworks.Asset
{
	public class BundleBuilderWindow : EditorWindow
	{
		string inputPath	= "";
		string outputPath	= "";

		BuildTarget target = BuildTarget.StandaloneWindows;

		BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;

		[MenuItem("Tools/BundleBuilder")]
		public static void OpenBundleBuilderWindow()
		{
			var window = GetWindow<BundleBuilderWindow>(false, "Bundle Builder", true);
			window.minSize = new Vector2(600,600);
			window.Show();
		}

		private bool SelectFile(ref string filePath, string Title, string extName)
		{
			string path = "";

			path = EditorUtility.OpenFilePanel(Title, "Assets", extName);

			if (path != "")
			{
				filePath = path;
				return true;
			}

			return false;
		}

		private bool SelectFolder(ref string folderPath, string title)
		{
			string path = EditorUtility.OpenFolderPanel(title, "", "");
			if (path != "")
			{
				folderPath = path;
				return true;
			}

			return false;
		}

		void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("打包资源根目录");
			inputPath = EditorGUILayout.TextField(inputPath);
			if(GUILayout.Button("..."))
			{
				string selectPath = "";
				if ( SelectFolder(ref selectPath, "打包资源根目录"))
				{
					if (!selectPath.StartsWith(Application.dataPath))
					{
						Debug.LogError("打包资源必须是包含于项目内的路径");
						
					}
					else
					{
						inputPath = selectPath.Substring(Application.dataPath.Length - "/Assets".Length + 1, selectPath.Length - (Application.dataPath.Length - "/Assets".Length + 1)).Replace("\\","/");
					}

				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("输出Bundle根目录");
			outputPath = EditorGUILayout.TextField(outputPath);
			if (GUILayout.Button("..."))
			{
				SelectFolder(ref outputPath, "输出Bundle根目录");				
			}
			EditorGUILayout.EndHorizontal();

			//target = (BuildTarget)EditorGUILayout.EnumFlagsField("Bundle平台:",target);
			target = (BuildTarget)EditorGUILayout.EnumPopup("Bundle平台:", target);

			buildOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("buildOptions:", buildOptions);

			if (GUILayout.Button("Build Bundle"))
			{
				string outputPlatformPath = string.Format("{0}/{1}", outputPath, target.ToString());
				if (!Directory.Exists(outputPlatformPath))
					Directory.CreateDirectory(outputPlatformPath);

				BundleBuilder.BuildAssets(inputPath, outputPlatformPath, buildOptions, target);
			}

			if (GUILayout.Button("MarkAssetBundle"))
			{
				string outputPlatformPath = string.Format("{0}/{1}", outputPath, target.ToString());
				if (!Directory.Exists(outputPlatformPath))
					Directory.CreateDirectory(outputPlatformPath);

				BundleBuilder.MarkAssetBundleAysn(inputPath, true);
			}

			if (GUILayout.Button("ClearMarkAssetBundle"))
			{
				string outputPlatformPath = string.Format("{0}/{1}", outputPath, target.ToString());
				if (!Directory.Exists(outputPlatformPath))
					Directory.CreateDirectory(outputPlatformPath);

				BundleBuilder.MarkAssetBundleAysn(inputPath, false);
			}
		}
	}
}
