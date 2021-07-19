using UnityEngine;
using UnityEditor;

public static class EditorFileAPI
{
	public static bool SelectFile(ref string filePath, string title, string extName)
	{
		string path = "";

		path = EditorUtility.OpenFilePanel(title, "Assets", extName);

		if (path != "")
		{
			filePath = path;
			return true;
		}

		return false;
	}

	public static bool SelectFile(ref string filePath, string title, string extName, string defaultPath)
	{
		string path = "";

		path = EditorUtility.OpenFilePanel(title, defaultPath, extName);

		if (path != "")
		{
			filePath = path;
			return true;
		}

		return false;
	}

	public static bool SelectFolder(ref string folderPath, string title)
	{
		if (string.IsNullOrEmpty(title))
		{
			title = "";
		}

		string path = EditorUtility.OpenFolderPanel(title, "", folderPath ?? "");
		if (path != "")
		{
			folderPath = path;
			return true;
		}

		return false;
	}
}
