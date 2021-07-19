//CSV转Json格式
// 前三行分别对应 
// 数据变量名		
// 注释描述		
// 数据类型		
/*
id,fData,strData,V2Data,V3Data,V4Data,LInt,CollectStrInt
整型,浮点数,字符串,二维向量,三维向量,四维向量,整型数组,组合数据
int,float,string,Vector2,Vector3,Vector4,Array<int>,"Collect<string name,int index>"
1,1.09,name,"23.0_33","1_2_3","1_2_3_4","1;2;3;4;5","name1,1;name2,2;name3,3"
*/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frameworks.Json
{

	public class CfgElementType
	{
		public static readonly string[] ms_BaseType = { "int", "float", "string", "Vector2", "Vector3", "Vector4" };

		public static readonly string[] ms_ScalaBaseType = { "Int", "Float", "String", "Vector2", "Vector3", "Vector4" };

		public string name = "";

		public string type = "";
		public string listObjType = "";

		public enum ElementBaseType
		{
			Error = -1,
			Int,
			Float,
			String,
			Vector2,
			Vector3,
			Vector4,
		}

		public ElementBaseType baseType = ElementBaseType.Error;

		public enum ElementTypeType
		{
			Base,
			Array,
			Collect,
			Error
		}

		public ElementTypeType typeType = ElementTypeType.Error;

		public CfgElementType collectType = null;

		public List<CfgElementType> _params = null;

		public void SetType(string nameParam, string typeParam, ref Dictionary<string, CfgElementType> typeTable)
		{
			for (int i = 0; i < ms_BaseType.Length; ++i)
			{
				if (typeParam.CompareTo(ms_BaseType[i]) == 0)
				{
					type = ms_BaseType[i];
					baseType = (ElementBaseType)i;
					typeType = ElementTypeType.Base;
					collectType = null;
					return;
				}
			}

			int ArrayTypeBeginIndex = typeParam.IndexOf('<');
			int ArrayTypeEndIndex = typeParam.IndexOf('>');

			if (ArrayTypeBeginIndex == -1 || ArrayTypeEndIndex == -1)
				return;

			if ( typeParam.StartsWith("Array") )
			{
				listObjType = typeParam.Substring(ArrayTypeBeginIndex + 1, ArrayTypeEndIndex - ArrayTypeBeginIndex - 1);
				type = string.Format("List<{0}>", listObjType);

				baseType = (ElementBaseType)System.Array.FindIndex<string>(ms_BaseType, val => { return val.CompareTo(listObjType) == 0; });

				typeType = ElementTypeType.Array;
				collectType = null;
				return;
			}

			if (!typeParam.StartsWith("Collect"))
				return;

			var typeData = typeParam.Substring(ArrayTypeBeginIndex + 1, ArrayTypeEndIndex - ArrayTypeBeginIndex - 1).Split(',');

			collectType = new CfgElementType();
			collectType._params = new List<CfgElementType>();
			collectType.type = nameParam;

			for (int i = 0;  i < typeData.Length; ++i)
			{
				var arrayTypeToName = typeData[i].Split(' ');
				CfgElementType paramType = new CfgElementType();
				paramType.SetType( nameParam, arrayTypeToName[0], ref typeTable); 
				paramType.name = arrayTypeToName[1];

				collectType._params.Add(paramType);
				collectType.type += string.Format("_{0}_{1}", paramType.type, paramType.name);
			}

			if (!typeTable.ContainsKey(typeParam))
				typeTable.Add(typeParam, collectType);

			type = string.Format("List<{0}>", collectType.type);
			listObjType = collectType.type;
			typeType = ElementTypeType.Collect;

		}

		public void SetScalaType(string nameParam, string typeParam, ref Dictionary<string, CfgElementType> typeTable)
		{
			for (int i = 0; i < ms_ScalaBaseType.Length; ++i)
			{
				if (typeParam.CompareTo(ms_BaseType[i]) == 0) // using the same type name in config file as C# version
				{
					type = ms_ScalaBaseType[i];
					baseType = (ElementBaseType)i;
					typeType = ElementTypeType.Base;
					collectType = null;
					return;
				}
			}

			int ArrayTypeBeginIndex = typeParam.IndexOf('<');
			int ArrayTypeEndIndex = typeParam.IndexOf('>');

			if (ArrayTypeBeginIndex == -1 || ArrayTypeEndIndex == -1)
				return;

			if (typeParam.StartsWith("Array"))
			{
				listObjType = typeParam.Substring(ArrayTypeBeginIndex + 1, ArrayTypeEndIndex - ArrayTypeBeginIndex - 1);
				baseType = (ElementBaseType)System.Array.FindIndex<string>(ms_BaseType, val => { return val.CompareTo(listObjType) == 0; });

				listObjType = listObjType[0].ToString().ToUpper() + listObjType.Substring(1);

				type = string.Format("Array[{0}]", listObjType);
				typeType = ElementTypeType.Array;
				collectType = null;
				return;
			}

			if (!typeParam.StartsWith("Collect"))
				return;

			var typeData = typeParam.Substring(ArrayTypeBeginIndex + 1, ArrayTypeEndIndex - ArrayTypeBeginIndex - 1).Split(',');

			collectType = new CfgElementType();
			collectType._params = new List<CfgElementType>();
			collectType.type = nameParam;

			for (int i = 0; i < typeData.Length; ++i)
			{
				var arrayTypeToName = typeData[i].Split(' ');
				CfgElementType paramType = new CfgElementType();
				paramType.SetScalaType(nameParam, arrayTypeToName[0], ref typeTable);
				paramType.name = arrayTypeToName[1];

				collectType._params.Add(paramType);
				collectType.type += string.Format("_{0}_{1}", paramType.type, paramType.name);
			}

			if (!typeTable.ContainsKey(typeParam))
				typeTable.Add(typeParam, collectType);

			type = string.Format("Array[{0}]", collectType.type);
			listObjType = collectType.type;
			typeType = ElementTypeType.Collect;

		}
	}

	public static class JsonTools
	{
		public const string ms_GeneratorConfigNameSpace = "Config";

		public static bool ConvertCSVToJsonFormat(string csvPath, string jsonPath)
		{
			if (string.IsNullOrEmpty(csvPath)
			|| string.IsNullOrEmpty(jsonPath))
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error,csvPath == {0} and jsonPath == {1}", csvPath, jsonPath);
				return false;
			}

			if (!File.Exists(csvPath))
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} is not exist!", csvPath);
				return false;
			}

			string csvText = File.ReadAllText(csvPath);

			var csvData = Frameworks.CSV.CSVBase.CreateCSV(csvText);

			if (csvData == null)
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} read failed!", csvPath);
			}

			var colName = csvData.LineData[0];
			var colType = csvData.LineData[2];

			JObject jsonTotal = new JObject();

			for (int row = 3; row < csvData.LineData.Count; ++row)
			{
				var line = csvData.LineData[row];

				JObject json = new JObject();

				for (int col = 0; col < line.colData.Count; ++col)
				{
					var dataType = colType.colData[col];

					if (string.IsNullOrEmpty(dataType))
						continue;

					if (dataType.StartsWith("//"))
						continue;

					if (dataType.StartsWith("array"))
					{
						var obj = BuildJsonArray(line.colData[col]);
						json.Add( colName.colData[col], obj);
					}
					else if (dataType.StartsWith("collect"))
					{
						var obj = BuildJsonCollect( colType.colData[col], line.colData[col] );
						json.Add( colName.colData[col], obj);
					}
					else
					{
						json.Add( colName.colData[col], line.colData[col]);
					}


				}

				jsonTotal.Add(line.colData[0], json);
			}

			File.WriteAllText(jsonPath, jsonTotal.ToString());



			return true;
		}
	
		static JArray BuildJsonArray(string textData)
		{
			JArray rt = new JArray();

			var elements = textData.Split(';');

			for (int i = 0; i < elements.Length; ++i)
			{
				rt.Add(elements[i]);
			}

			return rt;
		}

		static JArray BuildJsonCollect(string textType, string textData)
		{
			JArray rt = new JArray();

			int beginIndex = textType.IndexOf('<');
			int endIndex = textType.IndexOf('>');
			var typeData = textType.Substring(beginIndex + 1, endIndex - beginIndex - 1).Split(',');

			var elements = textData.Split(';');

			for (int i = 0; i < elements.Length; ++i)
			{
				var subElement = elements[i].Split(',');

				if (subElement.Length != typeData.Length)
				{
					Debug.LogErrorFormat("Convert Json Error! {0} is not a Collect Data", elements[i]);
					continue;
				}

				var obj = new JObject();

				for (int j = 0; j < subElement.Length; ++j)
				{
					var elemTypeDef = typeData[j].Split(' ');
					obj.Add(elemTypeDef[1], subElement[j]);
				}

				rt.Add(obj);
			}

			return rt;
		}

		public static bool ConvertCSVToJson(string csvPath, string jsonPath)
		{
			if (string.IsNullOrEmpty(csvPath)
			|| string.IsNullOrEmpty(jsonPath))
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error,csvPath == {0} and jsonPath == {1}", csvPath, jsonPath);
				return false;
			}

			if (!File.Exists(csvPath))
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} is not exist!", csvPath);
				return false;
			}

			string csvText = File.ReadAllText(csvPath);

			var csvData = Frameworks.CSV.CSVBase.CreateCSV(csvText);

			if (csvData == null)
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} read failed!", csvPath);
			}

			var colName = csvData.LineData[0];
			var colType = csvData.LineData[2];

			string jsonText = "{\r\n";

			for (int row = 3; row < csvData.LineData.Count; ++row)
			{
				var line = csvData.LineData[row];

				if (line.colData.Count == 0)
				{
					Debug.LogErrorFormat("Csv File: {1} To Json Error: ColData is empty! row = {0}", row, csvPath);
					break;
				}

				jsonText += string.Format("\"{0}\":", line.colData[0]);

				jsonText += "{";

				for (int col = 0; col < line.colData.Count; ++col)
				{
					if (colType.colData.Count <= col)
					{
						Debug.LogErrorFormat("ConvertCSVToJson Error, colType.colData.Count <= col:{1} csvPath == {0}", csvPath, col);
						return false;
					}

					var dataType = colType.colData[col];

					if (string.IsNullOrEmpty(dataType))
						continue;

					if (dataType.StartsWith("//"))
						continue;

					if (dataType.StartsWith("Array"))
					{
						//var obj = BuildJsonArray(line.colData[col]);
						//json.Add(colName.colData[col], obj);

						jsonText += string.Format("\"{0}\":[{1}]", colName.colData[col], BuildJsonArrayText(line.colData[col]));
					}
					else if (dataType.StartsWith("Collect"))
					{
						//var obj = BuildJsonCollect(colType.colData[col], line.colData[col]);
						//json.Add(colName.colData[col], obj);
						jsonText += string.Format("\"{0}\":[{1}]", colName.colData[col], BuildJsonCollectText(colType.colData[col], line.colData[col]));
					}
					else
					{
						//json.Add(colName.colData[col], line.colData[col]);
						jsonText += string.Format("\"{0}\":\"{1}\"", colName.colData[col], line.colData[col].Replace("\r\n", "\n").Replace("\n","\\n"));
					}

					if (col != line.colData.Count - 1)
						jsonText += ",";
				}

				if (row != csvData.LineData.Count - 1)
					jsonText += "},\r\n";
				else
					jsonText += "}\r\n";
			}

			jsonText += "}";

			File.WriteAllText(jsonPath, jsonText);



			return true;
		}

		static string BuildJsonArrayText(string textData)
		{

			var elements = textData.Split(';');

			string rt = "";

			for (int i = 0; i < elements.Length; ++i)
			{
				if (i != elements.Length - 1)
				{
					rt += string.Format("\"{0}\",", elements[i].Replace("\r\n","\n").Replace("\n", "\\n"));
				}
				else
				{
					rt += string.Format("\"{0}\"", elements[i].Replace("\r\n", "\n").Replace("\n", "\\n"));
				}

			}

			return rt;
		}

		static string BuildJsonCollectText(string textType, string textData)
		{
			string rt = "";

			int beginIndex = textType.IndexOf('<');
			int endIndex = textType.IndexOf('>');
			var typeData = textType.Substring(beginIndex + 1, endIndex - beginIndex - 1).Split(',');

			var elements = textData.Split(';');

			for (int i = 0; i < elements.Length; ++i)
			{

				var subElement = elements[i].Split(',');

				if (subElement.Length != typeData.Length)
				{
					Debug.LogErrorFormat("Convert Json Error! {0} is not a Collect Data", elements[i]);
					continue;
				}

				rt += "{";

				for (int j = 0; j < subElement.Length; ++j)
				{
					var elemTypeDef = typeData[j].Split(' ');

					if (j != subElement.Length - 1)
						rt += string.Format("\"{0}\":\"{1}\",", elemTypeDef[1], subElement[j].Replace("\r\n", "\n").Replace("\n","\\n"));
					else
						rt += string.Format("\"{0}\":\"{1}\"", elemTypeDef[1], subElement[j].Replace("\r\n", "\n").Replace("\r\n", "\\n"));
				}

				if (i != elements.Length - 1)
				{
					rt += "},";
				}
				else 
				{
					rt += "}";
				}

			}

			return rt;
		}

		public static bool BuildJsonReaderFromCSV(string csvPath, string csPath)
		{
			if (string.IsNullOrEmpty(csvPath))
				return false;

			string csvText = File.ReadAllText(csvPath);

			var csvData = Frameworks.CSV.CSVBase.CreateCSV(csvText);

			if (csvData == null)
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} read failed!", csvPath);
				return false;
			}

			if (csvData.LineData.Count < 3 )
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} format failed! Line count < 3", csvPath);
				return false;
			}

			string FileName = Path.GetFileName(csPath);
			string FileNameWithOutExt = Path.GetFileNameWithoutExtension(csPath);

			string FileDataClassName = FileNameWithOutExt + "Data";
			string FileReaderClassName = FileNameWithOutExt + "Reader";

			List<CfgElementType> elementTypeInfo = new List<CfgElementType>();

			Dictionary<string, CfgElementType> paramStrToType = new Dictionary<string, CfgElementType>();

			var colName = csvData.LineData[0];
			var colType = csvData.LineData[2];

			if (colName.colData.Count != colType.colData.Count)
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} format failed! Name line not matched type line.", csvPath);
				return false;
			}


			for (int col = 0; col < colType.colData.Count; ++col)
			{
				var dataType = colType.colData[col];

				if (string.IsNullOrEmpty(dataType))
					continue;

				if (dataType.StartsWith("//"))
					continue;

				CfgElementType curElement = new CfgElementType();

				curElement.name = colName.colData[col];

				curElement.SetType( FileNameWithOutExt, dataType, ref paramStrToType);

				elementTypeInfo.Add(curElement);
			}



			string CodeText = "";

			#region Gen Using

			CodeText += "//This file has been automatically generated by Config Reader json tools.\r\n";
			//CodeText += "using System;\r\n";
			CodeText += "using System.Collections.Generic;\r\n";
			CodeText += "using UnityEngine;\r\n";
			CodeText += "using Newtonsoft.Json.Linq;\r\n";


			#endregion

			CodeText += string.Format( "namespace {0}\r\n{{\r\n", ms_GeneratorConfigNameSpace);

			#region Gen Data Class

			foreach (var paramDataClassIter in paramStrToType)
			{
				var ClassType = paramDataClassIter.Value;
				CodeText += string.Format("\tpublic partial class {0}\r\n\t{{\r\n", ClassType.type);

				for (int paramIndex = 0; paramIndex < ClassType._params.Count; ++paramIndex)
				{
					if (ClassType._params[paramIndex].typeType != CfgElementType.ElementTypeType.Base)
					{
						CodeText += string.Format("\t\tpublic {0} {1} = null;\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
					}
					else
					{ 
						switch ((ClassType._params[paramIndex].baseType))
						{
							case CfgElementType.ElementBaseType.String:
								{
									CodeText += string.Format("\t\tpublic {0} {1} = \"\";\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							case CfgElementType.ElementBaseType.Vector2:
								{
									CodeText += string.Format("\t\tpublic {0} {1} = Vector2.zero;\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							case CfgElementType.ElementBaseType.Vector3:
								{
									CodeText += string.Format("\t\tpublic {0} {1} = Vector3.zero;\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							case CfgElementType.ElementBaseType.Vector4:
								{
									CodeText += string.Format("\t\tpublic {0} {1} = Vector4.zero;\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							default:
								{
									CodeText += string.Format("\t\tpublic {0} {1};\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
						}
					} 


				}

				CodeText += "\t}\t\r\n\r\n";
			}

			CodeText += string.Format("\tpublic partial class {0}\r\n\t{{\r\n", FileDataClassName);

			for (int i = 0; i < elementTypeInfo.Count; ++i)
			{
				if (elementTypeInfo[i].typeType != CfgElementType.ElementTypeType.Base)
				{
					CodeText += string.Format("\t\tpublic {0} {1} = null;\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
				}
				else
				{
					switch (elementTypeInfo[i].baseType)
					{
						case CfgElementType.ElementBaseType.String:
							{
								CodeText += string.Format("\t\tpublic {0} {1} = \"\";\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						case CfgElementType.ElementBaseType.Vector2:
							{
								CodeText += string.Format("\t\tpublic {0} {1} = Vector2.zero;\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						case CfgElementType.ElementBaseType.Vector3:
							{
								CodeText += string.Format("\t\tpublic {0} {1} = Vector3.zero;\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						case CfgElementType.ElementBaseType.Vector4:
							{
								CodeText += string.Format("\t\tpublic {0} {1} = Vector4.zero;\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						default:
							{
								CodeText += string.Format("\t\tpublic {0} {1};\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
					}
				}
			
			}

			CodeText += "\t}\t\r\n\r\n";

			#endregion

			#region Gen Reader Class

			CodeText += string.Format("\tpublic partial class {0} : CfgFileBase\r\n\t{{\t\r\n", FileReaderClassName);

			CodeText += string.Format("\t\tpublic Dictionary<{0},{1}> DataInFile\r\n", elementTypeInfo[0].type, FileDataClassName);

			CodeText += "\t\t{\r\n";
			CodeText += "\t\t\tget\r\n";
			CodeText += "\t\t\t{\r\n";
			CodeText += "\t\t\t\treturn mDataInFile;\r\n";
			CodeText += "\t\t\t}\r\n";
			CodeText += "\t\t}\r\n";
			CodeText += string.Format("\r\n\t\tDictionary<{0},{1}> mDataInFile;\r\n", elementTypeInfo[0].type, FileDataClassName);

			CodeText += "\t\tpublic override bool Init(JToken jsonObj)\r\n";
			CodeText += "\t\t{\r\n";
			CodeText += "\t\t\tif (jsonObj == null)\r\n";
			CodeText += "\t\t\t\treturn false;\r\n";
			CodeText += "\r\n";

			CodeText += string.Format("\t\t\tmDataInFile = new Dictionary<{0}, {1}>();\r\n", elementTypeInfo[0].type, FileDataClassName);
			CodeText += "\t\t\tJToken temp = null;\r\n";
			CodeText += "\t\t\tforeach( var tokenNode in jsonObj)\r\n";
			CodeText += "\t\t\t{\r\n";
			CodeText += string.Format("\t\t\t\t{0} data = new {0}();\r\n", FileDataClassName);
			for (int i = 0; i < elementTypeInfo.Count; ++i)
			{
				var cur = elementTypeInfo[i];
				switch (cur.typeType)
				{
					case CfgElementType.ElementTypeType.Base:
						{
							CodeText += string.Format("\t\t\t\ttemp = tokenNode.First[\"{0}\"];\r\n", cur.name);

							CodeText += string.Format("\t\t\t\tif( temp != null)\r\n");
							CodeText += "\t\t\t\t{\r\n";

							switch (cur.baseType)
							{
								case CfgElementType.ElementBaseType.Vector2:
									{
										CodeText += string.Format("\t\t\t\t\tvar v2temp = temp.ToObject<string>().Split(\'_\');\r\n");
										CodeText += string.Format("\t\t\t\t\tif ( v2temp.Length != 2)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
										CodeText += string.Format("\t\t\t\t\tfloat f1 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat f2 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v2temp[0], out f1);\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v2temp[1], out f2);\r\n");
										CodeText += string.Format("\t\t\t\t\tdata.{0} = new Vector2(f1,f2);\r\n", cur.name);
									}
									break;
								case CfgElementType.ElementBaseType.Vector3:
									{
										CodeText += string.Format("\t\t\t\t\tvar v3temp = temp.ToObject<string>().Split(\'_\');\r\n");
										CodeText += string.Format("\t\t\t\t\tif ( v3temp.Length != 3)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
										CodeText += string.Format("\t\t\t\t\tfloat f1 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat f2 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat f3 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v3temp[0], out f1);\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v3temp[1], out f2);\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v3temp[2], out f3);\r\n");
										CodeText += string.Format("\t\t\t\t\tdata.{0} = new Vector3(f1,f2,f3);\r\n", cur.name);
									}
									break;
								case CfgElementType.ElementBaseType.Vector4:
									{
										CodeText += string.Format("\t\t\t\t\tvar v4temp = temp.ToObject<string>().Split(\'_\');\r\n");
										CodeText += string.Format("\t\t\t\t\tif ( v4temp.Length != 4)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
										CodeText += string.Format("\t\t\t\t\tfloat f1 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat f2 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat f3 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat f4 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v4temp[0], out f1);\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v4temp[1], out f2);\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v4temp[2], out f3);\r\n");
										CodeText += string.Format("\t\t\t\t\tfloat.TryParse(v4temp[3], out f4);\r\n");
										CodeText += string.Format("\t\t\t\t\tdata.{0} = new Vector4(f1,f2,f3,f4);\r\n", cur.name);
									}
									break;
								default:
									CodeText += string.Format("\t\t\t\t\tdata.{0} = temp.ToObject<{1}>();\r\n", cur.name, cur.type);
									break;
							}

							CodeText += "\t\t\t\t}\r\n";
						}
						break;
					case CfgElementType.ElementTypeType.Array:
						{
							CodeText += "\r\n";
							CodeText += string.Format("\t\t\t\tif (tokenNode.First[\"{0}\"] != null) \r\n", cur.name);
							CodeText += string.Format("\t\t\t\t{{\r\n");
							CodeText += string.Format("\t\t\t\t\tdata.{0} = new {1}();\r\n", cur.name, cur.type);
							CodeText += string.Format("\t\t\t\t\tforeach ( var subNode in tokenNode.First[\"{0}\"])\r\n", cur.name);
							CodeText += "\t\t\t\t\t{\r\n";

							switch (cur.baseType)
							{
								case CfgElementType.ElementBaseType.Vector2:
									{
										CodeText += string.Format("\t\t\t\t\t\tvar v2temp = subNode.ToObject<string>().Split(\'_\');\r\n");
										CodeText += string.Format("\t\t\t\t\t\tif ( v2temp.Length != 2)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
										CodeText += string.Format("\t\t\t\t\t\tfloat f1 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat f2 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v2temp[0], out f1);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v2temp[1], out f2);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tvar vData = new Vector2(f1,f2);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tdata.{0}.Add(vData);\r\n", cur.name);
									}
									break;
								case CfgElementType.ElementBaseType.Vector3:
									{
										CodeText += string.Format("\t\t\t\t\t\tvar v3temp = subNode.ToObject<string>().Split(\'_\');\r\n");
										CodeText += string.Format("\t\t\t\t\t\tif ( v3temp.Length != 3)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
										CodeText += string.Format("\t\t\t\t\t\tfloat f1 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat f2 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat f3 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v3temp[0], out f1);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v3temp[1], out f2);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v3temp[2], out f3);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tvar vData = new Vector3(f1,f2,f3);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tdata.{0}.Add(vData);\r\n", cur.name);
									}
									break;
								case CfgElementType.ElementBaseType.Vector4:
									{
										CodeText += string.Format("\t\t\t\t\t\tvar v4temp = subNode.ToObject<string>().Split(\'_\');\r\n");
										CodeText += string.Format("\t\t\t\t\t\tif ( v4temp.Length != 4)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
										CodeText += string.Format("\t\t\t\t\t\tfloat f1 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat f2 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat f3 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat f4 = 0.0f;\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[0], out f1);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[1], out f2);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[2], out f3);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[3], out f4);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tvar vData = new Vector4(f1,f2,f3,f4);\r\n");
										CodeText += string.Format("\t\t\t\t\t\tdata.{0}.Add(vData);\r\n", cur.name);
									}
									break;
								default:
									CodeText += string.Format("\t\t\t\t\t\tdata.{0}.Add(subNode.ToObject<{1}>());\r\n", cur.name, cur.listObjType);
									break;
							}


							CodeText += "\t\t\t\t\t}\r\n";
							CodeText += string.Format("\t\t\t\t}}\r\n");
							CodeText += "\r\n";
						}
						break;
					case CfgElementType.ElementTypeType.Collect:
						{
							CodeText += "\r\n";
							CodeText += string.Format("\t\t\t\tdata.{0} = new {1}();\r\n", cur.name, cur.type);
							CodeText += string.Format("\t\t\t\tif (tokenNode.First[\"{0}\"] != null) \r\n", cur.name);
							CodeText += string.Format("\t\t\t\t{{\r\n");
							CodeText += string.Format("\t\t\t\t\tforeach ( var subNode in tokenNode.First[\"{0}\"])\r\n", cur.name);
							CodeText += "\t\t\t\t\t{\r\n";
							CodeText += string.Format("\t\t\t\t\t\t{0} val = new {0}(); \r\n", cur.collectType.type);
							// 暂时只做到二级层次的 配置 如果需要更高配置再说，毕竟这样的配置可读性已经非常的糟糕，因此这里二级层次只处理 基础类型节点
							for (int j = 0; j < cur.collectType._params.Count; ++j)
							{
								var curParam = cur.collectType._params[j];

								switch (curParam.baseType)
								{
									case CfgElementType.ElementBaseType.Vector2:
										{
											CodeText += string.Format("\t\t\t\t\t\tvar v2temp = subNode[\"{0}\"].ToObject<string>().Split(\'_\');\r\n", curParam.name);
											CodeText += string.Format("\t\t\t\t\t\tif ( v2temp.Length != 2)\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
											CodeText += string.Format("\t\t\t\t\t\tfloat f1 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat f2 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v2temp[0], out f1);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v2temp[1], out f2);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tvar vData = new Vector2(f1,f2);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tval.{0} = vData;\r\n", curParam.name);
										}
										break;
									case CfgElementType.ElementBaseType.Vector3:
										{
											CodeText += string.Format("\t\t\t\t\t\tvar v3temp = subNode[\"{0}\"].ToObject<string>().Split(\'_\');\r\n", curParam.name);
											CodeText += string.Format("\t\t\t\t\t\tif ( v3temp.Length != 3)\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
											CodeText += string.Format("\t\t\t\t\t\tfloat f1 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat f2 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat f3 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v3temp[0], out f1);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v3temp[1], out f2);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v3temp[2], out f3);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tvar vData = new Vector3(f1,f2,f3);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tval.{0} = vData;\r\n", curParam.name);
										}
										break;
									case CfgElementType.ElementBaseType.Vector4:
										{
											CodeText += string.Format("\t\t\t\t\t\tvar v4temp = subNode[\"{0}\"].ToObject<string>().Split(\'_\');\r\n", curParam.name);
											CodeText += string.Format("\t\t\t\t\t\tif ( v4temp.Length != 4)\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\tDebug.LogErrorFormat(\"Read Json {0} Error!Parse Vector Data Error.\");\r\n", FileDataClassName);
											CodeText += string.Format("\t\t\t\t\t\tfloat f1 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat f2 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat f3 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat f4 = 0.0f;\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[0], out f1);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[1], out f2);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[2], out f3);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tfloat.TryParse(v4temp[3], out f4);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tvar vData = new Vector4(f1,f2,f3,f4);\r\n");
											CodeText += string.Format("\t\t\t\t\t\tval.{0} = vData;\r\n", curParam.name);
										}
										break;
									default:
										CodeText += string.Format("\t\t\t\t\t\tval.{0} = subNode[\"{0}\"].ToObject<{1}>(); \r\n", curParam.name, curParam.type);
										break;
								}

							}

							CodeText += string.Format("\t\t\t\t\t\tdata.{0}.Add(val);\r\n", cur.name);

							CodeText += "\t\t\t\t\t}\r\n";
							CodeText += string.Format("\t\t\t\t}}\r\n");
							CodeText += "\r\n";
						}
						break;
				}
				//            Query<string>("skill_define.json", skill.ID, "dsl");
			}

			if (elementTypeInfo[0].typeType != CfgElementType.ElementTypeType.Base || elementTypeInfo[0].type.Equals("string"))
			{
				CodeText += string.Format("\t\t\t\tif( data.{0} != null)\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\t\tif (mDataInFile.ContainsKey(data.{0}))\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\t\t\tDebug.LogErrorFormat(\"{0} Insert data to table failed! have the same key! key == {{0}}\", data.{1});\r\n", FileReaderClassName, elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t\t\tcontinue;\r\n");
				CodeText += string.Format("\t\t\t\t\t}}\r\n");
				CodeText += string.Format("\t\t\t\t\tmDataInFile.Add( data.{0}, data);\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t}}\r\n");
			}
			else
			{
				CodeText += string.Format("\t\t\t\tif (mDataInFile.ContainsKey(data.{0}))\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\t\tDebug.LogErrorFormat(\"{0} Insert data to table failed! have the same key! key == {{0}}\", data.{1});\r\n", FileReaderClassName, elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t\tcontinue;\r\n");
				CodeText += string.Format("\t\t\t\t}}\r\n");
				CodeText += string.Format("\t\t\t\tmDataInFile.Add( data.{0}, data);\r\n", elementTypeInfo[0].name);
			}

			CodeText += "\t\t\t}\r\n";
			CodeText += "\r\n";

			CodeText += "\t\t\treturn true;\r\n";
			CodeText += "\t\t}\r\n";

			//CodeText += "\t\tpublic void DoConfig()\r\n";
			//CodeText += "\t\t{\r\n";
			//CodeText += "\t\t\tfinishInitCallback?.Invoke();\r\n";
			//CodeText += "\t\t}\r\n";

			#endregion

			CodeText += "\t}\r\n";
			CodeText += "}";

			if (!Directory.Exists(Path.GetDirectoryName(csPath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(csPath));
			}

			File.WriteAllText(csPath, CodeText);

			return true;
		}

		public static bool BuildScalaJsonReaderFromCSV(string csvPath, string scalaPath, string packageName)
		{
			if (string.IsNullOrEmpty(csvPath))
				return false;

			string csvText = File.ReadAllText(csvPath);

			var csvData = Frameworks.CSV.CSVBase.CreateCSV(csvText);

			if (csvData == null)
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} read failed!", csvPath);
				return false;
			}

			if (csvData.LineData.Count < 3)
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} format failed! Line count < 3", csvPath);
				return false;
			}

			string FileName = Path.GetFileName(scalaPath);
			string FileNameWithOutExt = Path.GetFileNameWithoutExtension(scalaPath);

			string FileDataClassName = FileNameWithOutExt + "Data";
			string FileReaderClassName = FileNameWithOutExt + "Reader";

			List<CfgElementType> elementTypeInfo = new List<CfgElementType>();

			Dictionary<string, CfgElementType> paramStrToType = new Dictionary<string, CfgElementType>();

			var colName = csvData.LineData[0];
			var colType = csvData.LineData[2];

			if (colName.colData.Count != colType.colData.Count)
			{
				Debug.LogErrorFormat("ConvertCSVToJson Error, {0} format failed! Name line not matched type line.", csvPath);
				return false;
			}


			for (int col = 0; col < colType.colData.Count; ++col)
			{
				var dataType = colType.colData[col];

				if (string.IsNullOrEmpty(dataType))
					continue;

				if (dataType.StartsWith("//"))
					continue;

				CfgElementType curElement = new CfgElementType();

				curElement.name = colName.colData[col];

				curElement.SetScalaType(FileNameWithOutExt, dataType, ref paramStrToType);

				elementTypeInfo.Add(curElement);
			}



			string CodeText = "";

			#region Gen Using

			CodeText += "//This file has been automatically generated by Config Reader json tools.\r\n";
			CodeText += string.Format("package {0}\r\n", packageName);
			CodeText += "import java.io.File\r\n";
			CodeText += "import scala.collection.mutable.Map\r\n";
			CodeText += "import play.api.libs.json.{JsArray, JsObject, Json}\r\n\r\n";


			#endregion


			#region Gen Data Class

			foreach (var paramDataClassIter in paramStrToType)
			{
				var ClassType = paramDataClassIter.Value;
				CodeText += string.Format("class {0}\r\n{{\r\n", ClassType.type);

				for (int paramIndex = 0; paramIndex < ClassType._params.Count; ++paramIndex)
				{
					if (ClassType._params[paramIndex].typeType != CfgElementType.ElementTypeType.Base)
					{
						CodeText += string.Format("\tvar {1}:{0} = _\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
					}
					else
					{
						switch ((ClassType._params[paramIndex].baseType))
						{
							case CfgElementType.ElementBaseType.String:
								{
									CodeText += string.Format("\tvar {1}:{0} = \"\"\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							case CfgElementType.ElementBaseType.Vector2:
								{
									CodeText += string.Format("\tvar {1}:{0} = Vector2.zero\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							case CfgElementType.ElementBaseType.Vector3:
								{
									CodeText += string.Format("\tvar {1}:{0} = Vector3.zero\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							case CfgElementType.ElementBaseType.Vector4:
								{
									CodeText += string.Format("\tvar {1}:{0} = Vector4.zero\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
							default:
								{
									CodeText += string.Format("\tvar {1}:{0} = _\r\n", ClassType._params[paramIndex].type, ClassType._params[paramIndex].name);
								}
								break;
						}
					}


				}

				CodeText += "}\r\n\r\n";
			}

			CodeText += string.Format("class {0}\r\n{{\r\n", FileDataClassName);

			for (int i = 0; i < elementTypeInfo.Count; ++i)
			{
				if (elementTypeInfo[i].typeType != CfgElementType.ElementTypeType.Base)
				{
					CodeText += string.Format("\tvar {1} : {0} = _\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
				}
				else
				{
					switch (elementTypeInfo[i].baseType)
					{
						case CfgElementType.ElementBaseType.String:
							{
								CodeText += string.Format("\tvar {1} : {0} = \"\"\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						case CfgElementType.ElementBaseType.Vector2:
							{
								CodeText += string.Format("\tvar {1} : {0} = Vector2.zero\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						case CfgElementType.ElementBaseType.Vector3:
							{
								CodeText += string.Format("\tvar {1} : {0} = Vector3.zero\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						case CfgElementType.ElementBaseType.Vector4:
							{
								CodeText += string.Format("\tvar {1} : {0} = Vector4.zero\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
						default:
							{
								CodeText += string.Format("\tvar {1} : {0} = _\r\n", elementTypeInfo[i].type, elementTypeInfo[i].name);
							}
							break;
					}
				}

			}

			CodeText += "}\r\n\r\n";

			#endregion

			#region Gen Reader Class

			CodeText += string.Format("class {0} extends CfgFileBase\r\n{{\r\n", FileReaderClassName);

			CodeText += string.Format("\tprivate var m_DataInFile : Map[{0},{1}] = Map.empty \r\n", elementTypeInfo[0].type, FileDataClassName);

			CodeText += "\toverride def init( jsonObj: JsObject) : Unit =\r\n";
			CodeText += "\t{\r\n";

			CodeText += string.Format("\t\tm_DataInFile = Map.empty\r\n");

			CodeText += "\t\tvar jsonIter = jsonObj.values.iterator\r\n";
			CodeText += string.Format("\t\tvar strTemp : String = null\r\n", FileDataClassName);
			CodeText += string.Format("\t\tvar FArrayTemp : Array[Float] = null\r\n", FileDataClassName);
			CodeText += string.Format("\t\tvar JArrayTemp : JsArray = null\r\n", FileDataClassName);

			CodeText += "\t\twhile( jsonIter.hasNext )\r\n";
			CodeText += "\t\t{\r\n";
			
			CodeText += string.Format("\t\t\tvar next = jsonIter.next()\r\n");
			CodeText += string.Format("\t\t\tvar data = new {0}()\r\n", FileDataClassName);
			for (int i = 0; i < elementTypeInfo.Count; ++i)
			{
				var cur = elementTypeInfo[i];
				switch (cur.typeType)
				{
					case CfgElementType.ElementTypeType.Base:
						{
							switch (cur.baseType)
							{
								case CfgElementType.ElementBaseType.Vector2:
									{
										CodeText += string.Format("\t\t\tstrTemp = (next\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", cur.name);

										CodeText += string.Format("\t\t\tif (strTemp != null)\r\n");
										CodeText += string.Format("\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
										CodeText += string.Format("\t\t\t\tif ( FArrayTemp.length != 2)\r\n");
										CodeText += string.Format("\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\");\r\n", FileDataClassName, i + 1);
										CodeText += string.Format("\t\t\t\telse\r\n\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\tdata.{0} = new Vector2(FArrayTemp(0), FArrayTemp(1))\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t}}\r\n");
										CodeText += string.Format("\t\t\t}}\r\n");
									}
									break;
								case CfgElementType.ElementBaseType.Vector3:
									{
										CodeText += string.Format("\t\t\tstrTemp = (next\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", cur.name);

										CodeText += string.Format("\t\t\tif (strTemp != null)\r\n");
										CodeText += string.Format("\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
										CodeText += string.Format("\t\t\t\tif ( FArrayTemp.length != 3)\r\n");
										CodeText += string.Format("\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\");\r\n", FileDataClassName, i + 1);
										CodeText += string.Format("\t\t\t\telse\r\n\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\tdata.{0} = new Vector3(FArrayTemp(0), FArrayTemp(1), FArrayTemp(2))\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t}}\r\n");
										CodeText += string.Format("\t\t\t}}\r\n");
									}
									break;
								case CfgElementType.ElementBaseType.Vector4:
									{
										CodeText += string.Format("\t\t\tstrTemp = (next\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", cur.name);
										CodeText += string.Format("\t\t\tif (strTemp != null)\r\n", cur.name);
										CodeText += string.Format("\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
										CodeText += string.Format("\t\t\t\tif ( FArrayTemp.length != 4)\r\n");
										CodeText += string.Format("\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\");\r\n", FileDataClassName, i + 1);
										CodeText += string.Format("\t\t\t\telse\r\n\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\tdata.{0} = new Vector4(FArrayTemp(0), FArrayTemp(1), FArrayTemp(2), FArrayTemp(3))\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t}}\r\n");
										CodeText += string.Format("\t\t\t}}\r\n");
									}
									break;
								case CfgElementType.ElementBaseType.String:
									CodeText += string.Format("\t\t\tdata.{0} = (next\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", cur.name);
									break;
								default:
									CodeText += string.Format("\t\t\tdata.{0} = (next\\\"{0}\").asOpt[String].getOrElse(\"\").to{1}\r\n", cur.name, cur.baseType);
									break;
							}

						}
						break;
					case CfgElementType.ElementTypeType.Array:
						{
							CodeText += string.Format("\t\t\tJArrayTemp = (next\\\"{0}\").asOpt[JsArray].orNull\r\n", cur.name);
							CodeText += "\r\n";
							CodeText += string.Format("\t\t\tif (JArrayTemp != null) \r\n");
							CodeText += string.Format("\t\t\t{{\r\n");
							CodeText += string.Format("\t\t\t\tdata.{0} = new {1}(JArrayTemp.value.length)\r\n", cur.name, cur.type);
							CodeText += string.Format("\t\t\t\tvar i : Int = 0\r\n");
							CodeText += string.Format("\t\t\t\tfor ( subNode <- JArrayTemp.value)\r\n");
							CodeText += "\t\t\t\t{\r\n";

							switch (cur.baseType)
							{
								case CfgElementType.ElementBaseType.Vector2:
									{
										CodeText += string.Format("\t\t\t\t\tstrTemp = subNode.asOpt[String].getOrElse(\"\")\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t\tif (strTemp != null)\r\n");
										CodeText += string.Format("\t\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
										CodeText += string.Format("\t\t\t\t\t\tif ( FArrayTemp.length != 2)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\")\r\n", FileDataClassName, i + 1);
										CodeText += string.Format("\t\t\t\t\t\telse\r\n\t\t\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tdata.{0}(i) = new Vector2(FArrayTemp(0),FArrayTemp(1))\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t\t\t}}\r\n");
										CodeText += string.Format("\t\t\t\t\t}}\r\n");
									}
									break;
								case CfgElementType.ElementBaseType.Vector3:
									{
										CodeText += string.Format("\t\t\t\t\tstrTemp = subNode.asOpt[String].getOrElse(\"\")\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t\tif (strTemp != null)\r\n");
										CodeText += string.Format("\t\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
										CodeText += string.Format("\t\t\t\t\t\tif ( FArrayTemp.length != 3)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\")\r\n", FileDataClassName, i + 1);
										CodeText += string.Format("\t\t\t\t\t\telse\r\n\t\t\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tdata.{0}(i) = new Vector3(FArrayTemp(0),FArrayTemp(1),FArrayTemp(2))\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t\t\t}}\r\n");
										CodeText += string.Format("\t\t\t\t\t}}\r\n");
									}
									break;
								case CfgElementType.ElementBaseType.Vector4:
									{
										CodeText += string.Format("\t\t\t\t\tstrTemp = subNode.asOpt[String].getOrElse(\"\")\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t\tif (strTemp != null)\r\n");
										CodeText += string.Format("\t\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
										CodeText += string.Format("\t\t\t\t\t\tif ( FArrayTemp.length != 4)\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\")\r\n", FileDataClassName, i + 1);
										CodeText += string.Format("\t\t\t\t\t\telse\r\n\t\t\t\t\t\t{{\r\n");
										CodeText += string.Format("\t\t\t\t\t\t\tdata.{0}(i) = new Vector4(FArrayTemp(0),FArrayTemp(1),FArrayTemp(2),FArrayTemp(3))\r\n", cur.name);
										CodeText += string.Format("\t\t\t\t\t\t}}\r\n");
										CodeText += string.Format("\t\t\t\t\t}}\r\n");
									}
									break;
								case CfgElementType.ElementBaseType.String:
									CodeText += string.Format("\t\t\t\t\tdata.{0}(i) = subNode.asOpt[String].getOrElse(\"\")\r\n", cur.name, cur.listObjType);
									break;
								default:
									CodeText += string.Format("\t\t\t\t\tdata.{0}(i) = subNode.asOpt[String].getOrElse(\"\").to{1}\r\n", cur.name, cur.listObjType);
									break;
							}

							CodeText += "\t\t\t\t\ti += 1\r\n";
							CodeText += "\t\t\t\t}\r\n";
							CodeText += string.Format("\t\t\t}}\r\n");
							CodeText += "\r\n";
						}
						break;
					case CfgElementType.ElementTypeType.Collect:
						{
							CodeText += string.Format("\t\t\tJArrayTemp = (next\\\"{0}\").asOpt[JsArray].orNull\r\n", cur.name);
							CodeText += "\r\n";
							CodeText += string.Format("\t\t\tif (JArrayTemp != null) \r\n");
							CodeText += string.Format("\t\t\t{{\r\n");
							CodeText += string.Format("\t\t\t\tdata.{0} = new {1}(JArrayTemp.value.length);\r\n", cur.name, cur.type);
							CodeText += string.Format("\t\t\t\tvar i : Int = 0\r\n");

							CodeText += string.Format("\t\t\t\tfor ( subNode <- JArrayTemp.value)\r\n");
							CodeText += "\t\t\t\t{\r\n";
							CodeText += string.Format("\t\t\t\t\tvar _val = new {0}(); \r\n", cur.collectType.type);
							// 暂时只做到二级层次的 配置 如果需要更高配置再说，毕竟这样的配置可读性已经非常的糟糕，因此这里二级层次只处理 基础类型节点
							for (int j = 0; j < cur.collectType._params.Count; ++j)
							{
								var curParam = cur.collectType._params[j];

								switch (curParam.baseType)
								{
									case CfgElementType.ElementBaseType.Vector2:
										{
											CodeText += string.Format("\t\t\t\t\tstrTemp = (subNode\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", curParam.name);

											CodeText += string.Format("\t\t\t\t\tif (strTemp != null)\r\n");
											CodeText += string.Format("\t\t\t\t\t{{\r\n");
											CodeText += string.Format("\t\t\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
											CodeText += string.Format("\t\t\t\t\t\tif ( FArrayTemp.length != 2)\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\");\r\n", FileDataClassName, i + 1);
											CodeText += string.Format("\t\t\t\t\t\telse\r\n\t\t\t\t\t\t{{\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\t_val.{0} = new Vector2(FArrayTemp(0), FArrayTemp(1))\r\n", curParam.name);
											CodeText += string.Format("\t\t\t\t\t\t}}\r\n");
											CodeText += string.Format("\t\t\t\t\t}}\r\n");	
										}
										break;
									case CfgElementType.ElementBaseType.Vector3:
										{
											CodeText += string.Format("\t\t\t\t\tstrTemp = (subNode\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", curParam.name);

											CodeText += string.Format("\t\t\t\t\tif (strTemp != null)\r\n");
											CodeText += string.Format("\t\t\t\t\t{{\r\n");
											CodeText += string.Format("\t\t\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
											CodeText += string.Format("\t\t\t\t\t\tif ( FArrayTemp.length != 3)\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\");\r\n", FileDataClassName, i + 1);
											CodeText += string.Format("\t\t\t\t\t\telse\r\n\t\t\t\t\t\t{{\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\t_val.{0} = new Vector3(FArrayTemp(0), FArrayTemp(1), FArrayTemp(2))\r\n", curParam.name);
											CodeText += string.Format("\t\t\t\t\t\t}}\r\n");
											CodeText += string.Format("\t\t\t\t\t}}\r\n");
										}
										break;
									case CfgElementType.ElementBaseType.Vector4:
										{
											CodeText += string.Format("\t\t\t\t\tstrTemp = (subNode\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", curParam.name);

											CodeText += string.Format("\t\t\t\t\tif (strTemp != null)\r\n");
											CodeText += string.Format("\t\t\t\t\t{{\r\n");
											CodeText += string.Format("\t\t\t\t\t\tFArrayTemp = strTemp.split(\'_\').map(f => f.toFloat)\r\n");
											CodeText += string.Format("\t\t\t\t\t\tif ( FArrayTemp.length != 4)\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\tprintln(\"Read Json {0} Error!Parse Vector Data Error. Col = {1}\");\r\n", FileDataClassName, i + 1);
											CodeText += string.Format("\t\t\t\t\t\telse\r\n\t\t\t\t\t\t{{\r\n");
											CodeText += string.Format("\t\t\t\t\t\t\t_val.{0} = new Vector4(FArrayTemp(0), FArrayTemp(1), FArrayTemp(2), FArrayTemp(3))\r\n", curParam.name);
											CodeText += string.Format("\t\t\t\t\t\t}}\r\n");
											CodeText += string.Format("\t\t\t\t\t}}\r\n");
										}
										break;
									case CfgElementType.ElementBaseType.String:
										CodeText += string.Format("\t\t\t\t\t_val.{0} = (subNode\\\"{0}\").asOpt[String].getOrElse(\"\")\r\n", curParam.name, curParam.baseType);
										break;
									default:
										CodeText += string.Format("\t\t\t\t\t_val.{0} = (subNode\\\"{0}\").asOpt[String].getOrElse(\"\").to{1}\r\n", curParam.name, curParam.baseType);
										break;
								}

							}

							CodeText += string.Format("\t\t\t\t\tdata.{0}(i) = _val\r\n", cur.name);


							CodeText += "\t\t\t\t\ti += 1\r\n";
							CodeText += "\t\t\t\t}\r\n";
							CodeText += string.Format("\t\t\t}}\r\n");
							CodeText += "\r\n";
						}
						break;
				}
				//            Query<string>("skill_define.json", skill.ID, "dsl");
			}

			if (elementTypeInfo[0].typeType != CfgElementType.ElementTypeType.Base || elementTypeInfo[0].type.Equals("string"))
			{
				CodeText += string.Format("\t\t\tif( data.{0} != null)\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\tif (m_DataInFile.contains(data.{0}))\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\t\tprintln(\"{0} Insert data to table failed! have the same key! key == {{0}}\", data.{1})\r\n", FileReaderClassName, elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t}}\r\n");
				CodeText += string.Format("\t\t\t\telse\r\n");
				CodeText += string.Format("\t\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\t\tm_DataInFile += ( data.{0} -> data)\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t\t}}\r\n");
				CodeText += string.Format("\t\t\t}}\r\n");
			}
			else
			{
				CodeText += string.Format("\t\t\tif (m_DataInFile.contains(data.{0}))\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\tprintln(\"{0} Insert data to table failed! have the same key! key == {{0}}\", data.{1})\r\n", FileReaderClassName, elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t}}\r\n");
				CodeText += string.Format("\t\t\telse\r\n");
				CodeText += string.Format("\t\t\t{{\r\n");
				CodeText += string.Format("\t\t\t\tm_DataInFile += ( data.{0} -> data)\r\n", elementTypeInfo[0].name);
				CodeText += string.Format("\t\t\t}}\r\n");
			}

			CodeText += "\t\t}\r\n";
			CodeText += "\r\n";
			CodeText += "\t}\r\n";
			CodeText += "\r\n";
			CodeText += "\tdef DateInFile() = m_DataInFile\r\n";
			CodeText += "\r\n";

			CodeText += "}\r\n";


			#endregion

			if (!Directory.Exists(Path.GetDirectoryName(scalaPath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(scalaPath));
			}

			File.WriteAllText(scalaPath, CodeText);

			return true;
		}

		public static bool SaveJsonReaderInfoToHistory(string csvPath, string csPath)
		{
			string FileNameWithOutExt = Path.GetFileNameWithoutExtension(csPath);

			string FileReaderClassName = FileNameWithOutExt + "Reader";

			string readerHistoryDir = Application.dataPath + "/.History";

			string historyFile = readerHistoryDir + "/history.csv";

			if (!Directory.Exists(readerHistoryDir))
			{
				Directory.CreateDirectory(readerHistoryDir);
			}

			if (!File.Exists(historyFile))
			{
				string defaultHistoryFormat = "CSPath,CSVPath,ReaderClass\r\n,,\r\nstring,string,string\r\n";

				File.WriteAllText(historyFile, defaultHistoryFormat);
			}

			CSV.CSVBase csv = CSV.CSVBase.CreateCSV( File.ReadAllText(historyFile) );

			int index = csv.LineData.FindIndex(lineData => { return string.Compare(lineData.colData[1], csvPath, System.StringComparison.CurrentCulture) == 0; });

			if (index == -1)
			{
				CSV.CSVBase.CSVLineData lineData = new CSV.CSVBase.CSVLineData();

				lineData.colData.Add(csPath);
				lineData.colData.Add(csvPath);
				lineData.colData.Add(FileReaderClassName);

				csv.LineData.Add(lineData);
			}
			else
			{
				var lineData = csv.LineData[index];

				lineData.colData.Clear();

				lineData.colData.Add(csPath);
				lineData.colData.Add(csvPath);
				lineData.colData.Add(FileReaderClassName);
			}

			File.WriteAllText(historyFile, csv.ToString());

			return true;
		}
	}

}
