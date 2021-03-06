using UnityEngine;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Frameworks
{
	// Android data path using the unity3d persistentDataPath may be null or "" in some platform.
	// The reliable way to get the data path is using data/data/packagename.
	class FileIO
	{
		public static string AppName = "com.RainSword.SoulWeaphon";

		public static string DataPath
		{
			get
			{
				return m_DataPath;
			}
		}

	#if !UNITY_EDITOR
		static string m_DataPath = Application.persistentDataPath;
	#else
		static string m_DataPath = "./GameData";
	#endif

		public static bool InitDataPath()
		{
			try
			{
				if (!Directory.Exists(m_DataPath))
				{
					Directory.CreateDirectory(DataPath);
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);

	#if !UNITY_EDITOR
				m_DataPath = "data/data/com.RainSword.SoulWeaphon/GameData";
	#else
				m_DataPath = ".";
	#endif
				Debug.LogError(string.Format("Create Data Path Failed!Reset DataPath To {0}", DataPath));

				if (!Directory.Exists(DataPath))
				{
					Directory.CreateDirectory(DataPath);
				}

				return false;
			}

			return true;
		}

		public static bool CreateSubDirectory(string SubPath)
		{
			
			try
			{
				string fullPath = string.Format("{0}/{1}",DataPath,SubPath);
				Directory.CreateDirectory(fullPath);
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);
				return false;
			}

		}

		public static bool LoadToDataPath(string SubPath, ref byte[] dataArray)
		{
			try
			{
				dataArray = File.ReadAllBytes( string.Format("{0}/{1}",DataPath,SubPath));
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);
				return false;
			}
		}

		public static bool SaveToDataPath(string SubPath, byte[] dataArray)
		{
			try
			{
				File.WriteAllBytes( string.Format("{0}/{1}",DataPath,SubPath), dataArray);
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);
				return false;
			}
		}

		public static bool LoadToDataPath(string SubPath, ref string textAsset)
		{
			try
			{
				textAsset = File.ReadAllText( string.Format("{0}/{1}",DataPath,SubPath));
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);
				return false;
			}
		}

		public static bool SaveToDataPath(string SubPath, string textAsset)
		{
			try
			{
				File.WriteAllText( string.Format("{0}/{1}",DataPath,SubPath), textAsset);
				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.Message);
				return false;
			}
		}
	}
}
