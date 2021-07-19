using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Frameworks
{
	class EditorFilePath
	{
		public static string AssetPathToAbsotionPath(string assetPath)
		{
			const string assetPathStartStr = "Assets/";
			if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(assetPathStartStr))
				return "";

			return string.Format("{0}/{1}", Application.dataPath, assetPath.Substring(AssetPathStartStr.Length, assetPath.Length - AssetPathStartStr.Length));
		}

		public static string AbsotionPathToAssetPath(string absPath)
		{
			if (string.IsNullOrEmpty(absPath) || !absPath.StartsWith(Application.dataPath))
				return "";

			return string.Format("Assets{0}", absPath.Substring(Application.dataPath.Length, absPath.Length - Application.dataPath.Length));
		}

		public string absotionPath = "";
		public bool   isAssetPath = false;
		public string assetPath = "";

		public void Clear()
		{
			absotionPath = "";
			isAssetPath = false;
			assetPath = "";
		}

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
			if (string.IsNullOrEmpty(_assetPath))
				return;

			assetPath = _assetPath;
			isAssetPath = true;

			absotionPath = string.Format("{0}/{1}", Application.dataPath, _assetPath.Substring( AssetPathStartStr.Length, _assetPath.Length - AssetPathStartStr.Length) );
		}


		public void UISettingPath(string title, string ext = "*.*", string directory = "", bool IsSave = false, string defaultSaveName = "default")
		{
			string basePath  = string.Format( "{0}/{1}", Application.dataPath.Substring(0, Application.dataPath.Length - "/assets".Length), directory);

			string path = null;
			if (IsSave)
			{
				path = EditorUtility.SaveFilePanel(title, basePath, defaultSaveName, ext);
			}
			else
				path = EditorUtility.OpenFilePanel(title, basePath, ext);
			SettingFromAbsotionPath(path);
		}

		/// <summary>
		/// Call this funtion in the editor windows OnGUI function to active UI Control
		/// </summary>
		/// <param name="title"></param>
		/// <returns>Is Change path</returns>
		public bool OnGUI(string title, string ext = "*.*", string btnShow = "...",string directory = "", bool isSave = false, string defaultSaveName = "default")
		{
			string oldAbsotionPath = absotionPath.ToString();

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(title);
			absotionPath = EditorGUILayout.TextField(absotionPath);
			if (GUILayout.Button(btnShow))
			{
				UISettingPath(title, ext, directory, isSave, defaultSaveName);
			}
			GUILayout.EndHorizontal();
			//if (!string.IsNullOrEmpty(absotionPath))
			//{
			//	if (isAssetPath)
			//		EditorGUILayout.LabelField(string.Format("{0}:{1}", title, assetPath));
			//	else
			//		EditorGUILayout.LabelField(string.Format("{0}:{1}", title, absotionPath));
			//}
			GUILayout.EndVertical();

			return oldAbsotionPath.CompareTo(absotionPath) != 0;
		}
	}
}
