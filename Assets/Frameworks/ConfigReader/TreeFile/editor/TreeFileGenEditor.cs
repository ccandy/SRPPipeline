using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Frameworks.Common;

namespace Frameworks
{
	public class TreeFileGenEditor : EditorWindowBase<TreeFileGenEditor>
	{
		EditorFolderPath templaterPath	= new EditorFolderPath();

		EditorFolderPath genCSPath		= new EditorFolderPath();

		string NameSpace				= "Game.TreeGen";

		List<string> templateFilePath	= new List<string>();

		[MenuItem("Tools/TreeFileGenEditor", false, 10)]
		static void OpenEditor()
		{
			Open("TreeFileGenerator", new Vector2(600, 500));
		}

		private void OnEnable()
		{
			InitPath();
		}

		void InitPath()
		{
			templaterPath.SettingFromAbsotionPath(Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/Document/TreeFileTemplate");
			genCSPath.SettingFromAbsotionPath(Application.dataPath + "/Scripts/TreeFileGen");
		}

		void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			if (templaterPath.OnGUI("模板文件夹"))
			{

			}

			if (genCSPath.OnGUI("生成CS文件路径"))
			{

			}

			NameSpace = EditorGUILayout.TextField("命名空间：", NameSpace);


			if (GUILayout.Button("生成"))
			{
				DoGen(NameSpace);
			}

			EditorGUILayout.EndVertical();
		}

		string DoGenCreateObjectCS(string nameSpace)
		{
			string code = "using UnityEngine;\r\n";
			code += "using System.Collections.Generic;\r\n";
			
			bool hasNameSpace = !string.IsNullOrEmpty(NameSpace);

			string nextPreTab = "";

			if (hasNameSpace)
			{
				code += string.Format("namespace {0}\r\n{{\r\n", NameSpace);
				nextPreTab = "\t";

				TreeFileTemplate.CreateObjClassName = "TreeObjectFactory";
			}

			string nnPrefab   = nextPreTab + "\t";
			string nnnPrefab  = nnPrefab + "\t";
			string nnnnPrefab = nnnPrefab + "\t";

			code += string.Format("{0}public class {1}\r\n{0}{{\r\n", nextPreTab, TreeFileTemplate.CreateObjClassName);

			//code += string.Format("{0}public enum ClassTypeEnum\r\n", nnPrefab);
			//code += string.Format("{0}{{\r\n", nnPrefab);
			//for (int i = 0; i < TreeFileTemplate.TemplateCount; ++i)
			//{
			//	var template = TreeFileTemplate.GetTemplate(i);
			//	for (int j = 0; j < template.TreeClasses.Count; ++j)
			//	{
			//		var c = template.TreeClasses[j];
			//		code += string.Format("{0}e_{1} = {2},\r\n", nnnPrefab, c.ClassName, c.ClassIndex);
			//	}
			//}
			//code += string.Format("{0}}}\r\n", nnPrefab);

			code += string.Format("{0}delegate TreeData TreeDataCreateFunc();\r\n", nnPrefab);
			code += string.Format("{0}static Dictionary<string, TreeDataCreateFunc> ms_Funcs = null;\r\n", nnPrefab);

			code += string.Format("{0}static void CheckAndCreateFuncs()\r\n", nnPrefab);
			code += string.Format("{0}{{\r\n", nnPrefab);
			code += string.Format("{0}if (ms_Funcs != null)\r\n", nnnPrefab);
			code += string.Format("{0}return;\r\n", nnnnPrefab);
			code += string.Format("{0}ms_Funcs = new Dictionary<string, TreeDataCreateFunc>();\r\n", nnnPrefab);
			
			for (int i = 0; i < TreeFileTemplate.TemplateCount; ++i)
			{
				var template = TreeFileTemplate.GetTemplate(i);
				for (int j = 0; j < template.TreeClasses.Count; ++j)
				{
					var c = template.TreeClasses[j];
					code += string.Format("{0}ms_Funcs.Add( \"{1}\", ()=>{{ return new {1}();}});\r\n", nnnPrefab, c.ClassName);
					//code += string.Format("{0}\r\n", nnnPrefab);
				}
			}
			code += string.Format("{0}}}\r\n", nnPrefab);

			code += string.Format("{0}public static TreeData NewTreeDataObj(string _type)\r\n", nnPrefab);
			code += string.Format("{0}{{\r\n", nnPrefab);

			
			code += string.Format("{0}if ( string.IsNullOrEmpty(_type) )\r\n", nnnPrefab);
			code += string.Format("{0}{{\r\n", nnnPrefab);
			code += string.Format("{0}return null;\r\n", nnnnPrefab);
			code += string.Format("{0}}}\r\n", nnnPrefab);

			code += string.Format("{0}CheckAndCreateFuncs();\r\n", nnnPrefab);

			code += string.Format("{0}TreeDataCreateFunc func = null;\r\n", nnnPrefab);
			code += string.Format("{0}ms_Funcs.TryGetValue(_type, out func);\r\n", nnnPrefab);
			code += string.Format("{0}return func?.Invoke();\r\n", nnnPrefab);


			code += string.Format("{0}}}\r\n", nnPrefab);

			code += string.Format("{0}}}\r\n", nextPreTab);

			if (hasNameSpace)
			{
				code += string.Format("}}\r\n");
			}


			return code;
		}

		void DoGen(string nameSpace)
		{
			TreeFileTemplate.ClearTemplates();
;
			if ( string.IsNullOrEmpty(templaterPath.absotionPath) )
			{
				Log.Print(LogLevel.Error, "templaterPath Is Null Or Empty.");
				return;
			}

			if (!Directory.Exists(templaterPath.absotionPath))
			{
				Log.Print(LogLevel.Error, "templaterPath == {0} Is Not Exists.", templaterPath.absotionPath);
				return;
			}

			if (string.IsNullOrEmpty(genCSPath.absotionPath))
			{
				return;
			}

			if (!Directory.Exists(genCSPath.absotionPath))
			{
				Directory.CreateDirectory(genCSPath.absotionPath);
				if (!Directory.Exists(genCSPath.absotionPath))
				{
					Log.Print(LogLevel.Error, "{0} created failed!", genCSPath.absotionPath);
					return;
				}
			}

			string[] templates = Directory.GetFiles(templaterPath.absotionPath, "*.tree");

			for (int i = 0; i < templates.Length; ++i)
			{
				var treeFileText = File.ReadAllText(templates[i]);
				string templateName = Path.GetFileNameWithoutExtension(templates[i]);
				TreeFileTemplate template = TreeFileTemplate.Load(templateName, treeFileText);
				if (template != null)
				{
					TreeFileTemplate.AddTemplates(template);
				}
			}

			string CreateObjectCodePath = string.IsNullOrEmpty(NameSpace) ? genCSPath.absotionPath + "/TreeFile.cs" : genCSPath.absotionPath + "/" + NameSpace + ".cs";

			string createObjCode = DoGenCreateObjectCS(NameSpace);

			File.WriteAllText(CreateObjectCodePath, createObjCode);

			for (int i = 0; i < TreeFileTemplate.TemplateCount; ++i)
			{
				var template = TreeFileTemplate.GetTemplate(i);
				string csCodePath = string.Format("{0}/{1}.cs", genCSPath.absotionPath, template.TemplateName);
				string CSCode = template.GenCShare(nameSpace);
				File.WriteAllText( csCodePath, CSCode);
			}

			AssetDatabase.Refresh();
		}
	}
}
