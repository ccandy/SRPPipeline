using UnityEngine;
using UnityEditor;
using System.Collections;

public class GetAssetPath
{
	[MenuItem("Assets/Copy Asset Path To Clipboard",false, 10)]
	static private void GetAssetPathToClipboard()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);

		GUIUtility.systemCopyBuffer = path;
	}
}
