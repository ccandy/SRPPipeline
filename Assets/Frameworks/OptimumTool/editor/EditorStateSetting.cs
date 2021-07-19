using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Frameworks.OptimumTool
{
	public class EditorStateSetting : EditorWindowBase<EditorStateSetting>
	{
		string[] ShaderLODStr = new string[] { "100", "200", "300" };
		int[] ShaderLOD = new int[] { 100, 200, 300 };

		string keyword = "";

		[MenuItem("Tools/EditorStateSetting", false, 10)]
		static void OpenWindow()
		{
			Open("EditorStateSetting", new Vector2(600, 300));
		}

		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();
			GraphicsTier tier = (GraphicsTier)EditorGUILayout.EnumPopup("Graphics Tier: ", Graphics.activeTier);
			if (EditorGUI.EndChangeCheck())
			{
				Graphics.activeTier = tier;
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField( string.Format("Shader LOD: {0}", Shader.globalMaximumLOD));

			int lodIndex = Shader.globalMaximumLOD / 100 - 1;

			Mathf.Clamp(lodIndex, 0, ShaderLOD.Length - 1);

			EditorGUI.BeginChangeCheck();
			lodIndex = GUILayout.SelectionGrid(lodIndex, ShaderLODStr, ShaderLOD.Length);
			if (EditorGUI.EndChangeCheck())
			{
				Shader.globalMaximumLOD = (lodIndex + 1)*100;
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Shader Globe Key");

			keyword = EditorGUILayout.TextField("Keyword", keyword);
			bool isEnable = Shader.IsKeywordEnabled(keyword);

			EditorGUI.BeginChangeCheck();
			isEnable = EditorGUILayout.Toggle("Is Key Enable", isEnable);
			if (EditorGUI.EndChangeCheck())
			{
				if (isEnable)
					Shader.EnableKeyword(keyword);
				else
					Shader.DisableKeyword(keyword);
			}
			EditorGUILayout.Space();

			if (Selection.activeObject is Material)
			{
				EditorGUILayout.LabelField("Material Key");

				var mat = Selection.activeObject as Material;

				for (int i = 0; i < mat.shaderKeywords.Length; ++i)
				{
					var keyword = mat.shaderKeywords[i];
					EditorGUILayout.LabelField(string.Format("Material Key {0} enable", keyword));
					//	EditorGUI.BeginChangeCheck();
					//	isEnable = EditorGUILayout.Toggle(keyword, mat.IsKeywordEnabled(keyword));
					//	if (EditorGUI.EndChangeCheck())
					//	{
					//		if (isEnable)
					//			mat.EnableKeyword(keyword);
					//		else
					//			mat.DisableKeyword(keyword);
					//	}
				}
			}

			Repaint();
		}
	}
}
