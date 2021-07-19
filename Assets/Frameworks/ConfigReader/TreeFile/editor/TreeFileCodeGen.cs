
using Frameworks.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks
{
	public class TreeType
	{
		public readonly static string[] baseTypes = new string[]
		{
			"bool",
			"int",
			"float",
			"string",
			"AssetRef",
			"Vector2",
			"Vector3",
			"Vector4",
			"Vector2Int",
			"Vector3Int",
			"Curve",
		};

		public readonly static string[] baseCppTypes = new string[]
		{
			"bool",
			"int",
			"float",
			"std::string",
			"AssetRef",
			"Vector2",
			"Vector3",
			"Vector4",
			"Vector2Int",
			"Vector3Int",
			"Curve",
		};

		public static bool CheckIsBaseType(string typeName)
		{
			for (int i = 0; i < baseTypes.Length; ++i)
			{
				if (baseTypes[i].CompareTo(typeName) == 0)
					return true;
			}

			return false;
		}

		public string OrignType;

		public bool IsBaseType = false;

		public bool IsGenericType = false;

		public string ToCSharpType()
		{
			string code = "";

			if ( IsGenericType )
			{
				if (OrignType.StartsWith("Map"))
				{
					code = string.Format("Dictionary<{0},{1}>", KeyType.ToCSharpType(), ValType.ToCSharpType());
				}
				else if (OrignType.StartsWith("Array"))
				{
					code = string.Format("List<{0}>", ValType.ToCSharpType());
				}
				else if (OrignType.StartsWith("AssetRef"))
				{
					code = "AssetRef";
				}
				else
				{
					Log.Print(LogLevel.Error, "GenDefineCShareCode Read Error field type == {0}", OrignType);
					return code;
				}
			}
			else
			{
				code = OrignType;
			}

			return code;
		}

		TreeType()
		{ 
		}

		public static TreeType Create(string orignTypeName)
		{
			TreeType treeType		= new TreeType();

			treeType.OrignType		= orignTypeName;

			treeType.IsBaseType		= CheckIsBaseType(orignTypeName);
			treeType.IsGenericType	= GetGenericSubType(orignTypeName, ref treeType.KeyType, ref treeType.ValType);

			return treeType;
		}

		/// <summary>
		/// Map Key Type
		/// </summary>
		public TreeType KeyType;

		/// <summary>
		/// Map Value Type | Array Value Type
		/// </summary>
		public TreeType ValType;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fieldType"></param>
		/// <param name="keyType"></param>
		/// <param name="valType"></param>
		/// <returns>is Generic type</returns>
		public static bool GetGenericSubType(string fieldType, ref TreeType keyType, ref TreeType valType)
		{
			int typeStart = fieldType.IndexOf('<');
			int typeEnd = fieldType.LastIndexOf('>');

			bool isGenericType = typeStart != -1 && typeEnd != -1;

			if (isGenericType)
			{
				string subType = "";
				subType = fieldType.Substring(typeStart + 1, typeEnd - (typeStart + 1));
				subType = subType.Trim();

				int splitIndex = subType.IndexOf(',');
				int genericStartIndex = subType.IndexOf('<');

				if (splitIndex != -1 && (genericStartIndex == -1 || splitIndex < genericStartIndex))
				{
					keyType = Create( subType.Substring(0, splitIndex).Trim() );
					valType = Create( subType.Substring(splitIndex+1, subType.Length - splitIndex - 1).Trim() );
				}
				else
				{
					keyType = null;
					valType = Create(subType);
				}
			}
			else
			{
				keyType = null;
				valType = null;
			}

			return isGenericType;
		}
	}

	public class TreeTypeField
	{
		public TreeType Type;

		public string OrignName;

		TreeTypeField()
		{ 
		}

		public static TreeTypeField CreateTreeType(string typeName, string fieldName)
		{
			TreeTypeField field = new TreeTypeField();
			field.Type			= TreeType.Create(typeName);
			field.OrignName		= fieldName;

			return field;
		}

		public static TreeTypeField CreateTreeType(TreeType type, string fieldName)
		{
			TreeTypeField field = new TreeTypeField();
			field.Type = type;
			field.OrignName = fieldName;

			return field;
		}

		#region Gen Define


		public static string GenDefineCShareCode(string preTab, TreeTypeField field)
		{
			string code = "";

			if (field.Type.IsGenericType)
			{
				if (field.Type.OrignType.StartsWith("AssetRef"))
					code = string.Format("{2}public {0} {1} = null;\r\n", field.Type.ToCSharpType(), field.OrignName, preTab);
				else
					code = string.Format("{2}public {0} {1} = new {0}();\r\n", field.Type.ToCSharpType(), field.OrignName, preTab);
			}
			else
			{
				code = string.Format("{2}public {0} {1};\r\n", field.Type.ToCSharpType(), field.OrignName, preTab);
			}

			return code;
		}

		#endregion

		#region Gen Load
		public static string GenLoadBaseTypeCShareCode(string preTab, TreeType fieldType, string fieldName, string JObjectName, string jsonFieldName)
		{
			string code = "";
			string fieldTypeName = fieldType.ToCSharpType();
			
			if (fieldTypeName.ToLower().CompareTo("bool") == 0)
				code += string.Format("{0}{1} = ({2}[\"{3}\"]?.ToObject<int>()??0) != 0;\r\n", preTab, fieldName, JObjectName, jsonFieldName);
			else if (fieldTypeName.ToLower().CompareTo("int") == 0)
				code += string.Format("{0}{1} = {2}[\"{3}\"]?.ToObject<int>()??0;\r\n", preTab, fieldName, JObjectName, jsonFieldName);
			else if (fieldTypeName.ToLower().CompareTo("float") == 0)
				code += string.Format("{0}{1} = {2}[\"{3}\"]?.ToObject<float>()??0.0f;\r\n", preTab, fieldName, JObjectName, jsonFieldName);
			else if (fieldTypeName.ToLower().CompareTo("string") == 0)
				code += string.Format("{0}{1} = {2}[\"{3}\"]?.ToObject<string>()??\"\";\r\n", preTab, fieldName, JObjectName, jsonFieldName);
			else if (fieldTypeName.StartsWith("AssetRef") )
			{
				string assetType = "UnityEngine.Object";

				if ( !string.IsNullOrEmpty(fieldType.ValType.OrignType) )
				{
					assetType = fieldType.ValType.OrignType;
				}

				code += string.Format("{0}{1} = new AssetRef({2}[\"{3}\"]?.ToObject<string>()??\"\", typeof({4}) );\r\n", preTab, fieldName, JObjectName, jsonFieldName, assetType);
			}
			else if (fieldTypeName.CompareTo("Vector2") == 0
				|| fieldTypeName.CompareTo("Vector3") == 0
				|| fieldTypeName.CompareTo("Vector4") == 0
				|| fieldTypeName.CompareTo("Vector2Int") == 0
				|| fieldTypeName.CompareTo("Vector3Int") == 0)
				code += string.Format("{0}{1} = JsonStreamWork.JsonTo{2}({3}[\"{4}\"]);\r\n", preTab, fieldName, fieldTypeName, JObjectName, jsonFieldName);
			else if (fieldTypeName.CompareTo("AnimationCurve") == 0)
				code += string.Format("{0}{1} = JsonStreamWork.JsonToCurve({2}[\"{3}\"]);\r\n", preTab, fieldName, JObjectName, jsonFieldName);

			return code;
		}

		public static string GenLoadTreeClassCShareCode(string preTab, TreeClass fieldClass, string fieldName, string JObjectName, string jsonFieldName)
		{
			string code = "";

			string nPreTab = preTab + "\t";
			string nnPreTab = nPreTab + "\t";

			if (fieldClass == null)
				return code;

			code += string.Format("{0}if ({1}[\"{2}\"] != null)\r\n{0}{{\r\n", preTab, JObjectName, jsonFieldName);
			code += string.Format("{0}var _type{3} = {1}[\"{2}\"][\"_type\"]?.ToString() ?? \"\";\r\n", nPreTab, JObjectName, jsonFieldName, fieldName);
			code += string.Format("{0}{1} = {2}.NewTreeDataObj(_type{1}) as {3};\r\n", nPreTab, fieldName, TreeFileTemplate.CreateObjClassName, fieldClass.ClassName);
			code += string.Format("{0}if ({1} == null)\r\n", nPreTab, fieldName, fieldClass.ClassName);
			code += string.Format("{0}{{\r\n", nPreTab);
			code += string.Format("{0}Debug.LogErrorFormat(\"Json Load Failed at JObjectName == {{0}} Path == {{1}}\", {1}, {1}.Path);\r\n", nnPreTab, JObjectName);
			code += string.Format("{0}return;\r\n", nnPreTab);
			code += string.Format("{0}}}\r\n", nPreTab);

			code += string.Format("{0}{1}.LoadFromJson({2}[\"{3}\"]);\r\n", nPreTab, fieldName, JObjectName, jsonFieldName);

			code += string.Format("{0}}}\r\n", preTab);

			return code;
		}

		public static string GenLoadCShareCode(string preTab, TreeType fieldType, string fieldName, string JObjectName, string jsonFieldName)
		{
			string code = "";

			string nPreTab = preTab + "\t";

			string nnPreTab = preTab + "\t\t";

			string fieldTypeName = fieldType.ToCSharpType();

			if (fieldType.IsGenericType)
			{
				if (fieldTypeName.StartsWith("AssetRef"))
				{
					return GenLoadBaseTypeCShareCode(preTab, fieldType, fieldName, JObjectName, jsonFieldName);
				}

				code += string.Format("{0}if ({1} == null)\r\n", preTab, fieldName);
				code += string.Format("{0}{{\r\n", preTab);
				code += string.Format("{0}{1} = new {2}();\r\n", nPreTab, fieldName, fieldTypeName);
				code += string.Format("{0}}}\r\n", preTab);

				code += string.Format("{0}{1}.Clear();\r\n", preTab, fieldName);
				code += string.Format("{0}var array_{1} = {2}[\"{3}\"] as JArray;\r\n", preTab, fieldName, JObjectName, jsonFieldName);
				code += string.Format("{0}if (array_{1} != null)\r\n{0}{{\r\n", preTab, fieldName);
				code += string.Format("{0}for (int i_{1} = 0; i_{1} < array_{1}.Count; ++i_{1} )\r\n", nPreTab, fieldName);
				code += string.Format("{0}{{\r\n", nPreTab);
				code += string.Format("{0}\r\n", nPreTab);

				code += string.Format("{0}var jObj_{1} = {2}[\"{3}\"];\r\n", nnPreTab, fieldName, JObjectName, jsonFieldName);

				string subKeyName = string.Format("key_{0}", fieldName);
				string subValName = string.Format("val_{0}", fieldName);

				if (fieldType.KeyType != null)
				{
					code += string.Format("{0}{1} {2} = default;\r\n", nnPreTab, fieldType.KeyType.ToCSharpType(), subKeyName);
				}

				code += string.Format("{0}{1} {2} = default;\r\n", nnPreTab, fieldType.ValType.ToCSharpType(), subValName);

				//string subKeyType = "";
				//string subValType = "";
				//bool isValGenericType = GetGenericSubType(valType, ref subKeyType, ref subValType);

				if (fieldType.KeyType != null)
				{
					code += GenLoadCShareCode( nnPreTab, fieldType.KeyType, subKeyName, string.Format("jObj_{0}", fieldName), "Key");
					code += GenLoadCShareCode( nnPreTab, fieldType.ValType, subValName, string.Format("jObj_{0}", fieldName), "Value");
					code += string.Format("{0}{1}.Add({2},{3});\r\n", nnPreTab, fieldName, subKeyName, subValName);
				}
				else
				{
					code += GenLoadCShareCode( nnPreTab, fieldType.ValType, subValName, string.Format("jObj_{0}[i_{0}]", fieldName), "Value");
					code += string.Format("{0}{1}.Add({2});\r\n", nnPreTab, fieldName, subValName);
				}

				code += string.Format("{0}\r\n", nPreTab);
				code += string.Format("{0}}}\r\n", nPreTab);
				code += string.Format("{0}}}\r\n", preTab);
			}
			else if (fieldType.IsBaseType)
			{
				return GenLoadBaseTypeCShareCode(preTab, fieldType, fieldName, JObjectName, jsonFieldName);
			}
			else
			{
				var treeClass = TreeFileTemplate.GetTreeClass(fieldType.OrignType);
				if (treeClass != null)
				{
					return GenLoadTreeClassCShareCode(preTab, treeClass, fieldName, JObjectName, jsonFieldName);
				}
			}

			return code;
		}
		#endregion

		#region Gen Save
		public static string GenSaveBaseTypeCShareCode(string preTab, TreeType fieldType, string fieldName, string JObjectName, string jsonFieldName)
		{
			string code = "";

			string fieldTypeName = fieldType.ToCSharpType();

			if (fieldTypeName.ToLower().CompareTo("bool") == 0)
				code += string.Format("{0}{1}.Add(\"{2}\", {3}? 1 : 0);\r\n", preTab, JObjectName, jsonFieldName, fieldName);
			else if (fieldTypeName.ToLower().CompareTo("int") == 0)
				code += string.Format("{0}{1}.Add(\"{2}\", {3});\r\n", preTab, JObjectName, jsonFieldName, fieldName);
			else if (fieldTypeName.ToLower().CompareTo("float") == 0)
				code += string.Format("{0}{1}.Add(\"{2}\", {3});\r\n", preTab, JObjectName, jsonFieldName, fieldName);
			else if (fieldTypeName.ToLower().CompareTo("string") == 0)
				code += string.Format("{0}{1}.Add(\"{2}\", {3});\r\n", preTab, JObjectName, jsonFieldName, fieldName);
			else if (fieldTypeName.CompareTo("AssetRef") == 0)
				code += string.Format("{0}{1}.Add(\"{2}\", {3}?.Path??\"\");\r\n", preTab, JObjectName, jsonFieldName, fieldName);
			else if (fieldTypeName.CompareTo("Vector2") == 0
				|| fieldTypeName.CompareTo("Vector3") == 0
				|| fieldTypeName.CompareTo("Vector4") == 0
				|| fieldTypeName.CompareTo("Vector2Int") == 0
				|| fieldTypeName.CompareTo("Vector3Int") == 0)
				code += string.Format("{0}{1}.Add(\"{2}\", JsonStreamWork.{3}ToJson({4}));\r\n", preTab, JObjectName, jsonFieldName, fieldTypeName, fieldName);
			else if (fieldTypeName.CompareTo("AnimationCurve") == 0)
				code += string.Format("{0}{1}.Add(\"{2}\", JsonStreamWork.CurveToJson({3}));\r\n", preTab, JObjectName, jsonFieldName, fieldName);

			return code;
		}

		public static string GenSaveTreeClassCShareCode(string preTab, TreeClass fieldClass, string fieldName, string JObjectName, string jsonFieldName)
		{
			string code = "";

			string nPreTab = preTab + "\t";

			if (fieldClass == null)
				return code;

			code += string.Format("{0}if ({1} != null)\r\n{0}{{\r\n", preTab, fieldName);

			code += string.Format("{0}var jObj = {1}.SaveToJson();\r\n", nPreTab, fieldName, JObjectName, jsonFieldName);

			code += string.Format("{0}{2}.Add(\"{3}\", jObj);\r\n", nPreTab, fieldName, JObjectName, jsonFieldName);

			code += string.Format("{0}}}\r\n", preTab);

			return code;
		}

		public static string GenSaveCShareCode(string preTab, TreeType fieldType, string fieldName, string JObjectName, string jsonFieldName)
		{
			string code = "";

			string nPreTab = preTab + "\t";

			string nnPreTab = preTab + "\t\t";

			string fieldTypeName = fieldType.ToCSharpType();

			if (fieldType.IsGenericType)
			{

				if (fieldTypeName.StartsWith("AssetRef"))
				{
					return GenSaveBaseTypeCShareCode(preTab, fieldType, fieldName, JObjectName, jsonFieldName);
				}

				code += string.Format("{0}if ({1} != null)\r\n", preTab, fieldName);
				code += string.Format("{0}{{\r\n", preTab);
				code += string.Format("{0}JArray array_{1} = new JArray();\r\n", nPreTab, fieldName);
				string elemStr = string.Format("elem_{0}", fieldName);

				code += string.Format("{0}foreach(var elem_{1} in {1})\r\n", nPreTab, fieldName, fieldType);
				code += string.Format("{0}{{\r\n", nPreTab);

				string jElemStr = string.Format("jElem_{0}", fieldName);

				code += string.Format("{0}JObject {1} = new JObject();\r\n", nnPreTab, jElemStr);

				if (fieldType.KeyType != null)
				{
					code += string.Format("{0}var {1}_Key = {1}.Key;\r\n", nnPreTab, elemStr);
					code += string.Format("{0}var {1}_Value = {1}.Value;\r\n", nnPreTab, elemStr);

					code += GenSaveBaseTypeCShareCode(nnPreTab, fieldType.KeyType, elemStr + "_Key", jElemStr, "Key");
					code += GenSaveCShareCode(nnPreTab, fieldType.ValType, elemStr + "_Value", jElemStr, "Value");
				}
				else
				{
					code += GenSaveCShareCode(nnPreTab, fieldType.ValType, elemStr, jElemStr, "Value");
				}

				code += string.Format("{0}array_{1}.Add(jElem_{1});\r\n", nnPreTab, fieldName);
				code += string.Format("{0}}}\r\n", nPreTab);
				code += string.Format("{0}{2}.Add(\"{3}\", array_{1});\r\n", nPreTab, fieldName, JObjectName, jsonFieldName);
				code += string.Format("{0}}}\r\n", preTab);
			}
			else if ( fieldType.IsBaseType)
			{
				code += GenSaveBaseTypeCShareCode(preTab, fieldType, fieldName, JObjectName, jsonFieldName);
			}
			else
			{
				var treeClass = TreeFileTemplate.GetTreeClass(fieldType.OrignType);
				if (treeClass != null)
				{
					return GenSaveTreeClassCShareCode(preTab, treeClass, fieldName, JObjectName, jsonFieldName);
				}
			}

			return code;
		}
		#endregion

	}

	public class TreeClass
	{
		public string ClassName;

		public int ClassIndex;

		public string BaseClassName = "";

		public List<TreeTypeField> Fields = new List<TreeTypeField>();

		#region Gen Load AssetRef
		bool CheckHasAssetRefElement(TreeType treeType)
		{
			bool hasAssetRef = false;
			if (treeType.IsBaseType)
			{
				return treeType.OrignType.StartsWith("AssetRef");
			}
			else if (treeType.IsGenericType)
			{
				if (treeType.KeyType != null)
				{
					hasAssetRef |= CheckHasAssetRefElement(treeType.KeyType);
				}

				if (treeType.ValType != null)
				{
					hasAssetRef |= treeType.OrignType.StartsWith("AssetRef");
					if (!hasAssetRef)
						hasAssetRef |= CheckHasAssetRefElement(treeType.ValType);
				}
			}
			else
			{
				var treeClass = TreeFileTemplate.GetTreeClass(treeType.OrignType);
				if (treeClass == null)
					return hasAssetRef;

				for (int i = 0; i < treeClass.Fields.Count; ++i)
				{
					hasAssetRef |= CheckHasAssetRefElement(treeClass.Fields[i].Type);
				}
			}

			return hasAssetRef;
		}

		public void GenFieldLoadAssetRefCShareCode(ref string code, string preTab, TreeType fieldType, string fieldName)
		{
			bool hasAssetRefElem = CheckHasAssetRefElement(fieldType);

			if (!hasAssetRefElem)
				return;

			string nPreTab = preTab + "\t";

			if (fieldType.IsBaseType)
			{
				code += string.Format("{0}{1}.DoLoadAsset();\r\n", preTab, fieldName);
			}
			else if (fieldType.IsGenericType)
			{
				if (fieldType.OrignType.StartsWith("AssetRef"))
				{
					code += string.Format("{0}{1}.DoLoadAsset();\r\n", preTab, fieldName);
				}
				else
				{
					code += string.Format("{0}foreach(var elem_{1} in {1})\r\n", preTab, fieldName);
					code += string.Format("{0}{{\r\n", preTab);

					if (fieldType.KeyType != null)
					{
						code += string.Format("{0}var elem_{1}_key = elem_{1}.Key;\r\n", nPreTab, fieldName);
						GenFieldLoadAssetRefCShareCode(ref code, preTab + "\t", fieldType.KeyType, string.Concat( "elem_", fieldName, "_key") );
					}

					if (fieldType.ValType != null)
					{
						if (fieldType.KeyType != null)
							code += string.Format("{0}var elem_{1}_value = elem_{1}.Value;\r\n", nPreTab, fieldName);

						GenFieldLoadAssetRefCShareCode(ref code, preTab + "\t", fieldType.ValType, fieldType.KeyType != null ? string.Concat("elem_", fieldName, "_value") : string.Concat("elem_", fieldName) );
					}
					code += string.Format("{0}}}\r\n", preTab);
				}
			}
			else
			{
				code += string.Format("{0}{1}.LoadAssetRef();\r\n", preTab, fieldName);
			}

		}


		public void GenFieldLoadAssetRefAsyncCShareCode(ref string code, string preTab, TreeType fieldType, string fieldName)
		{
			bool hasAssetRefElem = CheckHasAssetRefElement(fieldType);

			if (!hasAssetRefElem)
				return;

			string nPreTab = preTab + "\t";

			if (fieldType.IsBaseType)
			{
				code += string.Format("{0}{1}.DoLoadAssetAsync();\r\n", preTab, fieldName);
			}
			else if (fieldType.IsGenericType)
			{
				if (fieldType.OrignType.StartsWith("AssetRef"))
				{
					code += string.Format("{0}{1}.DoLoadAssetAsync();\r\n", preTab, fieldName);
				}
				else
				{
					code += string.Format("{0}foreach(var elem_{1} in {1})\r\n", preTab, fieldName);
					code += string.Format("{0}{{\r\n", preTab);

					if (fieldType.KeyType != null)
					{
						code += string.Format("{0}var elem_{1}_key = elem_{1}.Key;\r\n", nPreTab, fieldName);
						GenFieldLoadAssetRefAsyncCShareCode(ref code, preTab + "\t", fieldType.KeyType, string.Concat("elem_", fieldName, "_key"));
					}

					if (fieldType.ValType != null)
					{
						if (fieldType.KeyType != null)
							code += string.Format("{0}var elem_{1}_value = elem_{1}.Value;\r\n", nPreTab, fieldName);

						GenFieldLoadAssetRefAsyncCShareCode(ref code, preTab + "\t", fieldType.ValType, fieldType.KeyType != null ? string.Concat("elem_", fieldName, "_value") : string.Concat("elem_", fieldName));
					}
					code += string.Format("{0}}}\r\n", preTab);
				}
			}
			else
			{
				code += string.Format("{0}{1}.LoadAssetRefAsync();\r\n", preTab, fieldName);
			}

		}

		public int GenFieldGetAssetRefCountCShareCode(ref string code, string preTab, TreeType fieldType, string fieldName)
		{
			bool hasAssetRefElem = CheckHasAssetRefElement(fieldType);

			if (!hasAssetRefElem)
				return 0;

			int count = 0;

			string nPreTab = preTab + "\t";

			if (fieldType.IsBaseType)
			{
				code += string.Format("{0}count += {1}.GetAssetRefCount();\r\n", preTab, fieldName);
			}
			else if (fieldType.IsGenericType)
			{
				if (fieldType.OrignType.StartsWith("AssetRef"))
				{
					code += string.Format("{0}// {1}count\r\n", preTab, fieldName);
					return 1;
				}
				else
				{
					bool isKeyAssetRef = false;
					bool isValueAssetRef = false;
					bool needForEach = false;
					if (fieldType.KeyType != null)
					{
						isKeyAssetRef = fieldType.KeyType.OrignType.StartsWith("AssetRef");
						needForEach |= !fieldType.KeyType.IsBaseType;
					}

					if (fieldType.ValType != null)
					{
						isValueAssetRef = fieldType.ValType.OrignType.StartsWith("AssetRef");
						needForEach |= !fieldType.KeyType.IsBaseType;
					}

					if (needForEach)
					{
						code += string.Format("{0}foreach(var elem_{1} in {1})\r\n", preTab, fieldName);
						code += string.Format("{0}{{\r\n", preTab);
						if (fieldType.KeyType != null && !isKeyAssetRef && !fieldType.KeyType.IsBaseType)
						{
							code += string.Format("{0}var elem_{1}_key = elem_{1}.Key;\r\n", nPreTab, fieldName);
							count += GenFieldGetAssetRefCountCShareCode(ref code, preTab + "\t", fieldType.KeyType, string.Concat("elem_", fieldName, "_key"));
						}

						if (fieldType.ValType != null && !isValueAssetRef && !fieldType.ValType.IsBaseType)
						{
							if (fieldType.KeyType != null)
								code += string.Format("{0}var elem_{1}_value = elem_{1}.Value;\r\n", nPreTab, fieldName);

							count += GenFieldGetAssetRefCountCShareCode(ref code, preTab + "\t", fieldType.ValType, fieldType.KeyType != null ? string.Concat("elem_", fieldName, "_value") : string.Concat("elem_", fieldName));
						}
						code += string.Format("{0}}}\r\n", preTab);
					}

					int mulCount = 0;
					mulCount += isKeyAssetRef ? 1 : 0;
					mulCount += isValueAssetRef ? 1 : 0;
					if (mulCount > 0)
					{
						code += string.Format("{0}count += {1}*{2}.Count;\r\n", preTab, mulCount, fieldName);
					}
										
				}
			}
			else
			{
				code += string.Format("{0}count += {1}.GetAssetRefCount();\r\n", preTab, fieldName);
			}

			return 0;
		}


		public void GenFieldDisposeAssetRefsCShareCode(ref string code, string preTab, TreeType fieldType, string fieldName)
		{
			bool hasAssetRefElem = CheckHasAssetRefElement(fieldType);

			if (!hasAssetRefElem)
				return;

			string nPreTab = preTab + "\t";

			if (fieldType.IsBaseType)
			{
				code += string.Format("{0}{1}.Dispose();\r\n", preTab, fieldName);
			}
			else if (fieldType.IsGenericType)
			{
				if (fieldType.OrignType.StartsWith("AssetRef"))
				{
					code += string.Format("{0}{1}.Dispose();\r\n", preTab, fieldName);
				}
				else
				{
					code += string.Format("{0}foreach(var elem_{1} in {1})\r\n", preTab, fieldName);
					code += string.Format("{0}{{\r\n", preTab);

					if (fieldType.KeyType != null)
					{
						code += string.Format("{0}var elem_{1}_key = elem_{1}.Key;\r\n", nPreTab, fieldName);
						GenFieldDisposeAssetRefsCShareCode(ref code, preTab + "\t", fieldType.KeyType, string.Concat("elem_", fieldName, "_key"));
					}

					if (fieldType.ValType != null)
					{
						if (fieldType.KeyType != null)
							code += string.Format("{0}var elem_{1}_value = elem_{1}.Value;\r\n", nPreTab, fieldName);

						GenFieldDisposeAssetRefsCShareCode(ref code, preTab + "\t", fieldType.ValType, fieldType.KeyType != null ? string.Concat("elem_", fieldName, "_value") : string.Concat("elem_", fieldName));
					}
					code += string.Format("{0}}}\r\n", preTab);
				}
			}
			else
			{
				code += string.Format("{0}{1}.DisposeAssetRefs();\r\n", preTab, fieldName);
			}
		}
		#endregion

		#region Editor OnGUI Function
		public void GenFieldOnGUICShareCode(ref string code, string preTab, TreeType fieldType, string fieldName)
		{

		}
		#endregion

		public string GenCShareCode(string preTab)
		{
			string code = "";

			if (string.IsNullOrEmpty(BaseClassName))
			{
				code += string.Format("{0}public partial class {1} : TreeData\r\n{0}{{{0}\r\n", preTab, ClassName);
			}
			else
			{
				code += string.Format("{0}public partial class {1} : {2}\r\n{0}{{{0}\r\n", preTab, ClassName, BaseClassName);
			}

			string nPreTab = preTab + "\t";
			string nnPreTab = preTab + "\t\t";
			for (int i = 0; i < Fields.Count; ++i)
			{
				code += TreeTypeField.GenDefineCShareCode(nPreTab, Fields[i]);
			}

			code += "\r\n";
			code += string.Format("{0}public override string ClassType()\r\n", nPreTab);
			code += string.Format("{0}{{\r\n", nPreTab);
			code += string.Format("{0}return \"{1}\";\r\n", nnPreTab, ClassName);
			code += string.Format("{0}}}\r\n", nPreTab);


			#region Gen Load
			code += "\r\n";
			code += string.Format("{0}public override void LoadFromJson(JToken jObject)\r\n", nPreTab);
			code += string.Format("{0}{{\r\n", nPreTab);
			code += string.Format("{0}base.LoadFromJson(jObject);\r\n", nnPreTab);
			for (int i = 0; i < Fields.Count; ++i)
			{
				code += TreeTypeField.GenLoadCShareCode(nnPreTab, Fields[i].Type, Fields[i].OrignName, "jObject", Fields[i].OrignName);
			}
			code += string.Format("{0}}}\r\n", nPreTab);
			#endregion

			#region Gen Save
			code += "\r\n";
			code += string.Format("{0}public override JObject SaveToJson()\r\n", nPreTab);
			code += string.Format("{0}{{\r\n", nPreTab);
			code += string.Format("{0}JObject jObject = base.SaveToJson();\r\n", nnPreTab);
			code += string.Format("{0}jObject[\"_type\"] = ClassType();\r\n", nnPreTab);

			for (int i = 0; i < Fields.Count; ++i)
			{
				code += TreeTypeField.GenSaveCShareCode(nnPreTab, Fields[i].Type, Fields[i].OrignName, "jObject", Fields[i].OrignName);
			}

			code += string.Format("{0}return jObject;\r\n", nnPreTab);
			code += string.Format("{0}}}\r\n", nPreTab);

			#endregion

			#region Gen Asset Reference Function
			code += "\r\n";
			code += string.Format("{0}public override void LoadAssetRef()\r\n", nPreTab);
			code += string.Format("{0}{{\r\n", nPreTab);
			code += string.Format("{0}base.LoadAssetRef();\r\n", nnPreTab);
			for (int i = 0; i < Fields.Count; ++i)
			{
				var field = Fields[i];
				GenFieldLoadAssetRefCShareCode(ref code, nnPreTab, field.Type, field.OrignName);
			}
			code += string.Format("{0}}}\r\n", nPreTab);

			code += "\r\n";
			code += string.Format("{0}public override void LoadAssetRefAsync()\r\n", nPreTab);
			code += string.Format("{0}{{\r\n", nPreTab);
			code += string.Format("{0}base.LoadAssetRefAsync();\r\n", nnPreTab);
			for (int i = 0; i < Fields.Count; ++i)
			{
				var field = Fields[i];
				GenFieldLoadAssetRefAsyncCShareCode(ref code, nPreTab + "\t", field.Type, field.OrignName);
			}
			code += string.Format("{0}}}\r\n", nPreTab);

			//code += "\r\n";
			//code += string.Format("{0}public override int GetAssetRefCount()\r\n", nPreTab);
			//code += string.Format("{0}{{\r\n", nPreTab);

			//code += string.Format("{0}int count = 0;\r\n", nnPreTab);
			//for (int i = 0; i < Fields.Count; ++i)
			//{
			//	var field = Fields[i];
			//	GenFieldGetAssetRefCountCShareCode(ref code, nnPreTab, field.Type, field.OrignName);
			//}
			//code += string.Format("{0}}}\r\n", nPreTab);

			code += "\r\n";
			code += string.Format("{0}public override void DisposeAssetRefs()\r\n", nPreTab);
			code += string.Format("{0}{{\r\n", nPreTab);
			code += string.Format("{0}base.DisposeAssetRefs();\r\n", nnPreTab);
			for (int i = 0; i < Fields.Count; ++i)
			{
				var field = Fields[i];
				GenFieldDisposeAssetRefsCShareCode(ref code, nPreTab + "\t", field.Type, field.OrignName);
			}
			code += string.Format("{0}}}\r\n", nPreTab);

			#endregion

			//#region Gen Editor OnGUI Function
			//code += "\r\n";
			//code += "#ifdef UNITY_EDITOR\r\n";
			//code += string.Format("{0}public override void OnGUI()\r\n", nPreTab);
			//code += string.Format("{0}{{\r\n", nPreTab);
			//code += string.Format("{0}base.OnGUI();\r\n", nnPreTab);
			//for (int i = 0; i < Fields.Count; ++i)
			//{
			//	var field = Fields[i];
			//	GenFieldLoadAssetRefCShareCode(ref code, nPreTab + "\t", field.Type, field.OrignName);
			//}
			//code += string.Format("{0}}}\r\n", nPreTab);
			//code += "#endif\r\n";
			//code += "\r\n";
			//#endregion

			code += string.Format("{0}}}\r\n\r\n", preTab);

			return code;
		}

		public string GenCppHeadCode(string preTab)
		{
			string code = "";

			string nPreTab = preTab + "\t";
			string nnPreTab = preTab + "\t\t";

			if (string.IsNullOrEmpty(BaseClassName))
			{
				code += string.Format("{0}class {1} : TreeData\r\n{0}{{{0}\r\n", preTab, ClassName);
			}
			else
			{
				code += string.Format("{0}class {1} : {2}\r\n{0}{{{0}\r\n", preTab, ClassName, BaseClassName);
			}

			code += string.Format("{0}}}\r\n\r\n", preTab);
			return code;
		}

		public string GenCppSourceCode(string preTab)
		{
			string code = "";
			return code;
		}
	}

	public class TreeFileTemplate
	{
		static List<TreeFileTemplate> ms_TreeFileTemplates = new List<TreeFileTemplate>();

		public static int	 NextClassIndex = 0;

		public static string CreateObjClassName = "TreeData";

		public static void ClearTemplates()
		{
			if (ms_TreeFileTemplates != null)
				ms_TreeFileTemplates.Clear();

			NextClassIndex = 0;
		}

		public static int TemplateCount
		{
			get
			{
				return ms_TreeFileTemplates == null ? 0 : ms_TreeFileTemplates.Count;
			}
		}

		public static TreeFileTemplate GetTemplate(int index)
		{
			if (index >= ms_TreeFileTemplates.Count || ms_TreeFileTemplates == null || index < 0)
				return null;

			return ms_TreeFileTemplates[index];
		}

		public static bool AddTemplates(TreeFileTemplate newTemplate)
		{
			if (ms_TreeFileTemplates == null)
			{
				ms_TreeFileTemplates = new List<TreeFileTemplate>();
			}

			ms_TreeFileTemplates.Add(newTemplate);

			return true;
		}

		public static TreeClass GetTreeClass(string className)
		{
			TreeClass treeClass = null;

			for (int i = 0; i < ms_TreeFileTemplates.Count; ++i)
			{
				treeClass = ms_TreeFileTemplates[i].TreeClasses.Find(c => { return c.ClassName.CompareTo(className) == 0; });
				if (treeClass != null)
					return treeClass;
			}

			return treeClass;
		}

		string m_TemplateName = "";

		public string TemplateName
		{
			get
			{
				return m_TemplateName;
			}
		}

		protected TreeFileTemplate()
		{

		}

		public List<TreeClass> TreeClasses = new List<TreeClass>();

		public static TreeFileTemplate Load(string Name, string template)
		{
			TreeFileTemplate treeFileTemplate = new TreeFileTemplate();

			treeFileTemplate.m_TemplateName = Name;

			TreeClass treeClass = null;

			int defineBeginOffset = 0;

			var spliter = new char[] { ' ', '\t' };

			while(true)
			{
				int classDefineIndex = template.IndexOf("class", defineBeginOffset);

				if (classDefineIndex == -1)
					break;

				defineBeginOffset = classDefineIndex + 5;

				if (defineBeginOffset >= template.Length)
					break;

				if (template[defineBeginOffset] != ' ' && template[defineBeginOffset] != '\t')
				{
					Log.Print( LogLevel.Error, "template format Error At {0}, do not begin with a class define", defineBeginOffset);
					break;
				}

				int endDefineIndex = template.IndexOf('{', defineBeginOffset);
				if (endDefineIndex == -1)
				{
					Log.Print(LogLevel.Error, "template format Error At {0}, can't found class begin.", defineBeginOffset);
					break;
				}

				treeClass = new TreeClass();
				var classDefine = template.Substring(defineBeginOffset, endDefineIndex - defineBeginOffset).Trim();

				int parentClassBegin = classDefine.LastIndexOf(':');
				if (parentClassBegin == -1)
				{
					treeClass.ClassName = classDefine;
				}
				else
				{
					treeClass.ClassName		= classDefine.Substring(0, parentClassBegin).Trim();
					treeClass.BaseClassName = classDefine.Substring(parentClassBegin+1, classDefine.Length - parentClassBegin-1).Trim();
				}


				int endFieldindex = template.IndexOf('}', defineBeginOffset);
				if (endFieldindex == -1)
				{
					Log.Print(LogLevel.Error, "template format Error At {0}, can't found the end of the class define.", defineBeginOffset);
					break;
				}

				var fieldDefineStr = template.Substring(endDefineIndex+1, endFieldindex - endDefineIndex-1).Trim();

				var fieldDefineStrs = fieldDefineStr.Split(';');

				for (int i = 0; i < fieldDefineStrs.Length; ++i)
				{
					fieldDefineStrs[i] = fieldDefineStrs[i].Trim();

					int descriptBeginIndex = fieldDefineStrs[i].IndexOf("//");
					if (descriptBeginIndex != -1)
					{
						int descriptEndIndex = fieldDefineStrs[i].IndexOf('\n', descriptBeginIndex);
						if (descriptEndIndex != -1)
						{
							fieldDefineStrs[i] = fieldDefineStrs[i].Substring(0, descriptBeginIndex) + fieldDefineStrs[i].Substring(descriptEndIndex, fieldDefineStrs[i].Length - descriptEndIndex);
						}
						else
						{
							fieldDefineStrs[i] = fieldDefineStrs[i].Substring(0, descriptBeginIndex);
						}

					}

					var fieldDefine = fieldDefineStrs[i];

					int index = fieldDefine.LastIndexOfAny(spliter);
					if (index == -1)
					{
						continue;
					}

					var typeName = fieldDefine.Substring(0, index).Trim();

					if (string.IsNullOrEmpty(typeName))
						continue;

					var fieldName = fieldDefine.Substring( index+1, fieldDefine.Length - index - 1);
					if (string.IsNullOrEmpty(fieldName))
						continue;

					TreeTypeField field = TreeTypeField.CreateTreeType(typeName, fieldName);

					treeClass.Fields.Add(field);
				}

				treeClass.ClassIndex = TreeFileTemplate.NextClassIndex++;
				treeFileTemplate.TreeClasses.Add(treeClass);

				defineBeginOffset = endFieldindex + 1;
			}

			return treeFileTemplate;
		}

		public string GenCShare(string NameSpace)
		{
			string code = "using UnityEngine;\r\n";
			code += "using Frameworks;\r\n";
			code += "using Newtonsoft.Json.Linq;\r\n";
			code += "using System.Collections.Generic;\r\n";
			code += "using Frameworks.Asset;\r\n";

			string nextPreTab = "";

			bool hasNameSpace = !string.IsNullOrEmpty(NameSpace);

			if (hasNameSpace)
			{
				code += string.Format("namespace {0}\r\n{{\r\n", NameSpace);
				nextPreTab = "\t";
			}

			for (int i = 0; i < TreeClasses.Count; ++i)
			{
				code += TreeClasses[i].GenCShareCode(nextPreTab);
			}

			if (hasNameSpace)
			{
				code += string.Format("}}\r\n");
			}

			return code;
		}
	}

}