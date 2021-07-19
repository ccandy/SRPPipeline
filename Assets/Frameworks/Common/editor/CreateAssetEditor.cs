using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assets.Editor.Tools
{
	public class CreateAssetEditor : EditorWindow
	{
		static private string GetRelativeAssetsPath(string path)
		{
			return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
		}

		[MenuItem("Assets/Create As New Asset", false, 10)]
		static private void CreateAsNewAsset()
		{
			var selection = Selection.activeObject;
			if (selection == null)
				return;

			var path = AssetDatabase.GetAssetPath(selection);

			var index = path.LastIndexOf('.');

			if (index == -1)
				return;

			path = path.Remove(index) + "_copy.asset";

			AssetDatabase.CreateAsset( Object.Instantiate(selection), path);
		}

	}
}
