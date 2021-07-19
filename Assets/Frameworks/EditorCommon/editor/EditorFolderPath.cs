using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Frameworks
{
	public class EditorFolderPath
	{
		public string	absotionPath = "";
		public bool		isAssetPath	 = false;
		public string	assetPath	 = "";

		public void SettingFromAbsotionPath(string _absotionPath)
		{
			absotionPath = _absotionPath;
			isAssetPath = _absotionPath.StartsWith(Application.dataPath);
			if (isAssetPath)
				assetPath = string.Format("Assets{0}", absotionPath.Substring(Application.dataPath.Length, absotionPath.Length - Application.dataPath.Length));
			else
				assetPath = "";
		}

		const string AssetPathStartStr = "Assets/";

		public void SettingFromAssetPath(string _assetPath)
		{
			assetPath = _assetPath;
			isAssetPath = true;

			absotionPath = string.Format("{0}/{1}", Application.dataPath, _assetPath.Substring(AssetPathStartStr.Length, _assetPath.Length - AssetPathStartStr.Length));
		}

		/// <summary>
		/// Call this funtion in the editor windows OnGUI function to active UI Control
		/// </summary>
		/// <param name="title"></param>
		/// <returns>Is Change path</returns>
		public bool OnGUI(string title)
		{
			string oldAbsotionPath = absotionPath.ToString();

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(title))
			{
				string path = EditorUtility.OpenFolderPanel(title, absotionPath, "");
				SettingFromAbsotionPath(path);
			}
			GUILayout.EndHorizontal();
			if (!string.IsNullOrEmpty(absotionPath))
			{
				if (isAssetPath)
					EditorGUILayout.LabelField(string.Format("{0}:{1}", title, assetPath));
				else
					EditorGUILayout.LabelField(string.Format("{0}:{1}", title, absotionPath));
			}
			GUILayout.EndVertical();

			return oldAbsotionPath.CompareTo(absotionPath) != 0;
		}
	}
}
