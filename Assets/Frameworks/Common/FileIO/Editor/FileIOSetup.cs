using UnityEngine;

using System;
using System.Collections.Generic;
using UnityEditor;
using System.Runtime.InteropServices;

using System.IO;
using System.Threading;

namespace Framework
{
	static class FileIOSetup
	{
		static string FileIOTemplatePath = Application.dataPath + "/Frameworks/Common/FileIO/Editor/FileIOInitlializer.cst";

		public static void BuildFileIO()
		{
			string bundleIdentifier = PlayerSettings.applicationIdentifier;

			string FileIOTemplate = File.ReadAllText(FileIOTemplatePath);

			string FileIOInitializerCS = string.Format(FileIOTemplate, bundleIdentifier);

			File.WriteAllText(Application.dataPath + "/Frameworks/Common/FileIO/FileIO.cs", FileIOInitializerCS);

			AssetDatabase.Refresh();

			Debug.Log("FileIOInitializer.cs Created!");
		}

		[MenuItem("Window/FileIO/BuildFileIO")]
		public static void OnBuildFileIO(MenuCommand cmd)
		{
			BuildFileIO();
		}
		
	}
}
