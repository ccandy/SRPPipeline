
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Frameworks
{
	// TO DO: No Finish Yet
	public class JsonCoderFileIOWindow : EditorWindowBase<JsonCoderFileIOWindow>
	{
		public static void GenJsonCoderFile()
		{
			var types = typeof(JsonCoder).Assembly.GetTypes();

			var jsonCoderType = Array.FindAll<Type>(types, t => t.IsSubclassOf(typeof(JsonCoder)) && !t.IsAbstract );

			for (int i = 0; i < jsonCoderType.Length; ++i)
			{
				var curType = types[i];

				

				//curType.BaseType
			}
		}

		public string BuildPreStr(int count, char c)
		{
			string preStr = "";
			for (int i = 0; i < count; ++i)
			{
				preStr += c;
			}
			return preStr;
		}

		public string BuildPreTab(int tabNum)
		{
			return BuildPreStr(tabNum, '\t');
		}

		public string BaseFieldInitCodeGen(Type _type, string fieldName, int preTabNum)
		{
			string code = "";
			string preTab = BuildPreTab(preTabNum);

			if (_type == typeof(bool))
				code += string.Format("{0}{1} {2} = false;\r\n", preTab, _type.Name, fieldName);
			else if (_type == typeof(int))
				code += string.Format("{0}{1} {2} = 0;\r\n", preTab, _type.Name, fieldName);
			else if (_type == typeof(float))
				code += string.Format("{0}{1} {2} = 0.0f;\r\n", preTab, _type.Name, fieldName);
			else if (_type == typeof(Vector2)
				|| _type == typeof(Vector3)
				|| _type == typeof(Vector4)
				|| _type == typeof(Vector2Int)
				|| _type == typeof(Vector3Int))
				code += string.Format("{0}{1} {2} = {1}.zero;\r\n", preTab, _type.Name, fieldName);
			else if (_type.IsSubclassOf( typeof(JsonCoder) ))
			{
				code += string.Format("{0}{1} {2} = null;\r\n", preTab, _type.Name, fieldName);
			}
			else
			{
				code += string.Format("{0}{1} {2} = new {1}();\r\n", preTab, _type.Name, fieldName);
			}

			return code;
		}

		public string BaseFieldLoadCodeGen(Type _type, string fieldName, int preTabNum, string jObjectName = "json", int deep = 1)
		{
			string code = "";

			string preTab = BuildPreTab(preTabNum);

			if (_type == typeof(bool))
				code += string.Format("{0}{1} = ({2}[\"{1}\"]?.toObject<int>()??0) != 0;\r\n", preTab, fieldName, jObjectName);
			else if (_type == typeof(int))
				code += string.Format("{0}{1} = {2}[\"{1}\"]?.toObject<int>()??0;\r\n", preTab, fieldName, jObjectName);
			else if (_type == typeof(float))
				code += string.Format("{0}{1} = {2}[\"{1}\"]?.toObject<float>()??0.0f;\r\n", preTab, fieldName, jObjectName);
			else if (_type == typeof(string))
				code += string.Format("{0}{1} = {2}[\"{1}\"]?.toObject<string>()??\"\";\r\n", preTab, fieldName, jObjectName);
			else if (_type == typeof(Vector2)
				|| _type == typeof(Vector3)
				|| _type == typeof(Vector4)
				|| _type == typeof(Vector2Int)
				|| _type == typeof(Vector3Int))
				code += string.Format("{0}{1} = JsonStreamWork.JsonTo{2}({3}[\"{1}\"]);\r\n", preTab, fieldName, _type.Name, jObjectName);
			else if (_type == typeof(AnimationCurve))
				code += string.Format("{0}{1} = JsonStreamWork.JsonToCurve({2}[\"{1}\"]);\r\n", preTab, fieldName, jObjectName);
			else if (_type.IsGenericType)
			{
				string GenericTypeNodeName = string.Format("node{0}{1}", BuildPreStr(deep, '_'), fieldName);

				code += string.Format("{0}var {1} = {3}[\"{2}\"];\r\n", preTab, GenericTypeNodeName, fieldName, jObjectName);
				if (_type.GenericTypeArguments.Length == 1)
				{
					string nodeInCollectName = string.Format("node{0}", BuildPreStr(deep, '_'));
					string GenericTypeDataName = string.Format("data{0}{1}", BuildPreStr(deep, '_'));

					code += string.Format("{0}{1}.Clear();\r\n", preTab, fieldName);

					code += string.Format("{0}foreach(var {2} in {1})\r\n", preTab, GenericTypeNodeName, nodeInCollectName);
					{
						code += string.Format("{0}{{\r\n", preTab);
						code += string.Format("{0}\t\r\n", preTab);
						code += BaseFieldInitCodeGen(_type.GenericTypeArguments[0], GenericTypeDataName, preTabNum + 1);
						code += BaseFieldLoadCodeGen(_type.GenericTypeArguments[0], GenericTypeDataName, preTabNum + 1, nodeInCollectName, deep + 1);
						code += string.Format("{0}\t{1}.Add({2});\r\n", preTab, fieldName, nodeInCollectName);
						code += string.Format("{0}}}\r\n", preTab);
					}
				}
			}

			return code;
		}

		public void GenTypeCSFile(Type type)
		{
			string nameSpace = type.Name.Substring(0, type.Name.LastIndexOf('.'));

			string className = type.Name.Substring(nameSpace.Length + 1, type.Name.Length - nameSpace.Length - 1);

			var Fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | ~System.Reflection.BindingFlags.Static);

			string code = "using Newtonsoft.Json.Linq;\r\n";
			code += string.Format("namespace {0}/r/n{{\r\n", nameSpace);
			code += "\r\n";
			code += string.Format("\tpartial class {0}\r\n{{\r\n", className);

			#region Generate LoadFromJson

			code += string.Format("\t\tpublic override void LoadFromJson(JObject json)\r\n");
			code += string.Format("\t\t{{\r\n");
			code += string.Format("\t\t\tif (json == null)\r\n");
			code += string.Format("\t\t\t\treturn;\r\n");

			for (int i = 0; i < Fields.Length; ++i)
			{
				code += BaseFieldLoadCodeGen(Fields[i].FieldType, Fields[i].Name, 3);
			}

			code += string.Format("\t\t\t\r\n");
			code += string.Format("\t\t}}\r\n");
			#endregion

			code += string.Format("\t\t\t\r\n");

			code += string.Format("\t}}\r\n");
			code += "\r\n";
			code += string.Format("}}\r\n");
		}
	}
}