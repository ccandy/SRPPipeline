using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class FrameworksPacker : EditorWindow 
{ 
	private static bool IsFileOfType(string fileName, string type)
	{
		if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(type) || fileName.EndsWith(".meta"))
			return false;

		string[] typeArray = type.Split('|');
		for (int i = 0; i < typeArray.Length; i++)
		{
			if (typeArray[i] == "*.*")
				return true;
			typeArray[i] = typeArray[i].Substring(1, typeArray[i].Length - 1);
		}

		for (int i = 0; i < typeArray.Length; i++)
		{
			if (fileName.EndsWith(typeArray[i]))
				return true;
		}

		return false;
	}

	public static List<string> GetFilesFormFolder(string folder, string type, bool allDir)
	{
		List<string> result = new List<string>();
		if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(type))
			return result;

		string folderPath = Application.dataPath;
		if (folder[0] != '/')
			folderPath += "/";
		if (folder[folder.Length - 1] != '/')
			folder += "/";
		folderPath += folder;

		SearchOption searchOption = (allDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		string[] srcFileArray = Directory.GetFiles(folderPath, "*.*", searchOption);
		for (int i = 0; i < srcFileArray.Length; i++)
		{
			string filePath = srcFileArray[i];
			if (IsFileOfType(filePath, type))
			{
				filePath = filePath.Replace("\\", "/");
				filePath = filePath.Substring(filePath.IndexOf("Assets/"));
				result.Add(filePath);
			}
		}
		return result;
	}

	[MenuItem("FrameworksPacker/Pack")]
	static void Pack()
	{
		DoPack(".");
	}

	static void DoPack(string BasePath)
	{
		string[] ResPath = new string[]{
									"Frameworks",
									"Plugins",
								 };

		List<string> allDataPath = new List<string>();

		//allDataPath.Add("Assets/ChangeList.txt");
		for (int i = 0; i < ResPath.Length; ++i)
		{
			List<string> subDataPath = GetFilesFormFolder(ResPath[i], "*.*", true);

			allDataPath.AddRange(subDataPath);
		}

		string curVersion = UnityEditor.PlayerSettings.bundleVersion;

		UnityEditor.AssetDatabase.ExportPackage(allDataPath.ToArray(), BasePath + "/" + "GameFrameworks" + curVersion + ".unitypackage", ExportPackageOptions.IncludeDependencies);

	}
}
