using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Frameworks.Asset
{
	public class FileInfoData
	{
		public FileInfo			fileInfo		= null;
		public DirectoryInfo	directoryInfo	= null;
		public string			path			= "";
	}

	public class BundleBuilder
	{
		public static void MarkAssetBundle(string path, bool IsMark)
		{
			if (string.IsNullOrEmpty(path))
				return;

			if (!Directory.Exists(path))
				return;

			path = path.ToLower();
			path.Replace("\\", "/");

			var curDirectoryInfo = new DirectoryInfo(path);

			var FilesInfo = curDirectoryInfo.GetFiles();			

			if (IsMark)
			{
				for (int i = 0; i < FilesInfo.Length; ++i)
				{
					var curFile = FilesInfo[i];

					if (curFile.FullName.EndsWith("meta"))
						continue;

					string assetPath = string.Format("{0}/{1}", curDirectoryInfo, curFile.Name);

					var import = AssetImporter.GetAtPath(assetPath);
					if (import == null)
						continue;

					import.assetBundleName		= path;
					import.assetBundleVariant	= "bundle";
				}
			}
			else
			{
				for (int i = 0; i < FilesInfo.Length; ++i)
				{
					var curFile = FilesInfo[i];

					if (curFile.FullName.EndsWith("meta"))
						continue;

					var import = AssetImporter.GetAtPath(curFile.FullName);
					if (import == null)
						continue;

					import.assetBundleName		= "";
					import.assetBundleVariant	= "";
				}
			}

			var childrenDirectories = curDirectoryInfo.GetDirectories();

			for (int i = 0; i < childrenDirectories.Length; ++i)
			{
				var directory = childrenDirectories[i];

				string childPath = string.Format("{0}/{1}", path, directory.Name.ToLower());
				MarkAssetBundle( childPath, IsMark);
			}
		}

		public static void MarkAssetBundleAysn(string path, bool IsMark)
		{
			List<FileInfoData> fileInfoList = null;

			GetFileList(path, ref fileInfoList);

			if (fileInfoList == null || fileInfoList.Count == 0)
				return;

			int startIndex = 0;

			EditorApplication.update = () =>
			{
				var data = fileInfoList[startIndex];

				bool isCancel = EditorUtility.DisplayCancelableProgressBar("标记资源中", data.fileInfo.FullName, (float)startIndex / (float)fileInfoList.Count);

				string assetPath = string.Format("{0}/{1}", data.directoryInfo, data.fileInfo.Name);

				AssetImporter import = AssetImporter.GetAtPath(assetPath);

				if (data.fileInfo.FullName.EndsWith("meta")
				|| import == null)
				{
					
				}
				else
				{
					if (IsMark)
					{
						import.assetBundleName = data.path;
						import.assetBundleVariant = "bundle";
					}
					else
					{
						import.assetBundleVariant = "";
						import.assetBundleName = "";
					}
				}

				++startIndex;
				if (isCancel || startIndex >= fileInfoList.Count)
				{
					
					EditorUtility.ClearProgressBar();
					EditorApplication.update = null;
					startIndex = 0;
					AssetDatabase.RemoveUnusedAssetBundleNames();
					Debug.Log("标记结束");
				}
			};
		}

		public static void GetFileList(string basePath, ref List<FileInfoData> fileInfoList)
		{
			if (string.IsNullOrEmpty(basePath))
				return;

			if (!Directory.Exists(basePath))
				return;

			if (fileInfoList == null)
			{
				fileInfoList = new List<FileInfoData>();
			}

			basePath = basePath.ToLower();
			basePath.Replace("\\", "/");

			var curDirectoryInfo = new DirectoryInfo(basePath);

			var FilesInfo = curDirectoryInfo.GetFiles();

			foreach( var info in FilesInfo)
			{
				FileInfoData data = new FileInfoData();
				data.fileInfo = info;
				data.directoryInfo = curDirectoryInfo;
				data.path = basePath;
				fileInfoList.Add(data);
			}

			var childrenDirectories = curDirectoryInfo.GetDirectories();

			for (int i = 0; i < childrenDirectories.Length; ++i)
			{
				var directory = childrenDirectories[i];

				string childPath = string.Format("{0}/{1}", basePath, directory.Name.ToLower());
				GetFileList(childPath, ref fileInfoList);
			}
		}

		public static void BuildAssets(string inputPath, string outputPath, BuildAssetBundleOptions option, BuildTarget target)
		{
			//MarkAssetBundle(inputPath, true);
			BuildPipeline.BuildAssetBundles(outputPath, option | BuildAssetBundleOptions.DeterministicAssetBundle, target);
			//MarkAssetBundle(inputPath, false);
			AssetDatabase.RemoveUnusedAssetBundleNames();
		}
	}

}