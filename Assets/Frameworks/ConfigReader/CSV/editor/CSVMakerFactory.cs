#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Frameworks.CSV
{
	public class CSVMakerFactory
	{
		static string m_CSFileData = "";

		private static string GetFullPath(string srcName)
		{
			if (srcName.Equals(string.Empty))
			{
				return Application.dataPath;
			}

			if (srcName[0].Equals('/'))
			{
				srcName.Remove(0, 1);
			}

			return Application.dataPath + "/" + srcName;
		}

		static string GetStringFromTypeEnum(CSVType type)
		{
			switch (type)
			{
				case CSVType.INT:
					return "int";
				case CSVType.BOOL:
					return "bool";
				case CSVType.FLOAT:
					return "float";
				case CSVType.STRING:
					return "string";
				case CSVType.VECTOR3:
					return "Vector3";
				case CSVType.VECTOR2:
					return "Vector2";
			}

			return "";
		}

		static string GetGetDataFuncFromTypeEnum(CSVType type)
		{
			switch (type)
			{
				case CSVType.INT:
					return "GetIntData";
				case CSVType.BOOL:
					return "GetBoolData";
				case CSVType.FLOAT:
					return "GetFloatData";
				case CSVType.STRING:
					return "GetData";
				case CSVType.VECTOR3:
					return "GetVector3Data";
				case CSVType.VECTOR2:
					return "GetVector2Data";
			}

			return "";
		}

		public static bool BuildCSFileData(CSVFile curCSVFile,
											bool useKeyCol, int KeyCol,
											bool IsBuildResourceCreateFunction,
											bool IsBuildMemoryCreateFunction,
											bool IsBuildNewFunction,
											string CSFileNamePath)
		{
			m_CSFileData = "";

			// Add using.
			m_CSFileData += "using UnityEngine;\r\n"
						 + "using System;\r\n"
						 + "using System.Collections.Generic;\r\n"
						 + "using Frameworks.CSV;\r\n";

			string FileName = CSFileNamePath.Substring(CSFileNamePath.LastIndexOfAny(new char[] { '\\', '/' }) + 1);

			string FileNameWithOutExt = FileName.Split(new string[] { "." }, StringSplitOptions.None)[0];

			string FileDataClassName = FileNameWithOutExt + "Data";
			string FileReaderClassName = FileNameWithOutExt + "Reader";

			string FileDataClass = "public partial class " + FileDataClassName + "\r\n{\r\n";

			int colNum = curCSVFile.ColNum();
			string[] ColTypeString = new string[colNum];
			for (int col = 0; col < colNum; ++col)
			{
				ColTypeString[col] = GetStringFromTypeEnum(curCSVFile.GetColType(col));
				//	public type ColName;
				FileDataClass += "\tpublic " +
						ColTypeString[col] + " " + curCSVFile.GetColName(col) + ";\r\n";
			}


			FileDataClass += "};\r\n";

			string DicnaryString = "";

			if (KeyCol != -1)
			{
				DicnaryString = "Dictionary<"
								+ GetStringFromTypeEnum(curCSVFile.GetColType(KeyCol))
								+ "," + FileDataClassName + ">";
			}

			string FileReaderClass = "public partial class " + FileReaderClassName + "\r\n{\r\n";

			if (!useKeyCol)
			{
				FileReaderClass += "\tpublic " + FileDataClassName + "[] DataInFile;\r\n";
				#region CSVFileFunction
				FileReaderClass += "\tpublic static " + FileReaderClassName + " Create(CSVFile curCSVFile)\r\n"
							+ "\t{\r\n"
							+ "\t\t" + FileReaderClassName + " curReader = null;\r\n"
							+ "\t\tif (curCSVFile == null)\r\n"
							+ "\t\t\treturn curReader;\r\n"
							+ "\t\tcurReader = new " + FileReaderClassName + "();\r\n"
							+ "\t\tint rowNum = curCSVFile.RowNum();\r\n"
							+ "\t\tif (rowNum == 0)\r\n"
							+ "\t\t\treturn curReader;\r\n"
							+ "\t\tcurReader.DataInFile = new " + FileDataClassName + "[rowNum];\r\n"
							+ "\t\tfor(int row = 0; row < rowNum; ++row)\r\n"
							+ "\t\t{\r\n"
							+ "\t\t\t" + FileDataClassName + " curData = new " + FileDataClassName + "();\r\n";

				for (int col = 0; col < colNum; ++col)
				{
					//	curData.ColName = curCSVFile.GetTypeData( row, col);
					FileReaderClass += "\t\t\tcurData."
								+ curCSVFile.GetColName(col)
								+ " = curCSVFile."
								+ GetGetDataFuncFromTypeEnum(curCSVFile.GetColType(col))
								+ "( row," + col.ToString() + ");\r\n";
				}
				FileReaderClass += "\t\t\tcurReader.DataInFile[row] = curData;\r\n"
								+ "\t\t}\r\n"
								+ "\t\t\r\n"
								+ "\t\treturn curReader;\r\n"
								+ "\t}\r\n";
				#endregion
			}
			else
			{
				FileReaderClass += "\tpublic " + DicnaryString + " DataInFile;\r\n";

				#region CSVFileFunction
				FileReaderClass += "\tpublic static " + FileReaderClassName + " Create(CSVFile curCSVFile)\r\n"
							+ "\t{\r\n"
							+ "\t\t" + FileReaderClassName + " curReader = null;\r\n"
							+ "\t\tif (curCSVFile == null)\r\n"
							+ "\t\t\treturn curReader;\r\n"
							+ "\t\tcurReader = new " + FileReaderClassName + "();\r\n"
							+ "\t\tint rowNum = curCSVFile.RowNum();\r\n"
							+ "\t\tif (rowNum == 0)\r\n"
							+ "\t\t\treturn curReader;\r\n"
							+ "\t\tcurReader.DataInFile = new " + DicnaryString + "();\r\n"
							+ "\t\tfor(int row = 0; row < rowNum; ++row)\r\n"
							+ "\t\t{\r\n"
							+ "\t\t\t" + GetStringFromTypeEnum(curCSVFile.GetColType(KeyCol)) + " curKey = curCSVFile." + GetGetDataFuncFromTypeEnum(curCSVFile.GetColType(KeyCol)) + "( row," + KeyCol.ToString() + ");\r\n"
							+ "\t\t\tif (curReader.DataInFile.ContainsKey( curKey))\r\n"
							+ "\t\t\t{\r\n"
							+ "\t\t\t\tcontinue;\r\n"
							+ "\t\t\t}\r\n"
							+ "\t\t\t" + FileDataClassName + " curData = new " + FileDataClassName + "();\r\n";

				for (int col = 0; col < colNum; ++col)
				{
					//	curData.ColName = curCSVFile.GetTypeData( row, col);
					FileReaderClass += "\t\t\tcurData."
								+ curCSVFile.GetColName(col)
								+ " = curCSVFile."
								+ GetGetDataFuncFromTypeEnum(curCSVFile.GetColType(col))
								+ "( row," + col.ToString() + ");\r\n";
				}
				FileReaderClass += "\t\t\tcurReader.DataInFile.Add( curKey, curData);\r\n"
								+ "\t\t}\r\n"
								+ "\t\t\r\n"
								+ "\t\treturn curReader;\r\n"
								+ "\t}\r\n";
				#endregion
			}

			#region ResourceFunction
			if (IsBuildResourceCreateFunction)
			{
				FileReaderClass += "\tpublic static " + FileReaderClassName + " Create(string path)\r\n"
						+ "\t{\r\n"
						+ "\t\tCSVFile curCSVFile = CSVFile.CreateCSVFile(path);\r\n"
						+ "\t\tif (curCSVFile == null)\r\n"
						+ "\t\t\treturn null;\r\n"
						+ "\t\treturn Create(curCSVFile);\r\n"
						+ "\t}\r\n";
			}
			#endregion

			#region MemoryFunction
			if (IsBuildMemoryCreateFunction)
			{
				FileReaderClass += "\tpublic static " + FileReaderClassName + " CreateFromMemory(byte[] data)\r\n"
						+ "\t{\r\n"
						+ "\t\tCSVFile curCSVFile = CSVFile.CreateCSVFileFromMemory(data);\r\n"
						+ "\t\tif (curCSVFile == null)\r\n"
						+ "\t\t\treturn null;\r\n"
						+ "\t\treturn Create(curCSVFile);\r\n"
						+ "\t}\r\n";
			}
			#endregion

			#region BuildNewFunction And Save Function
			if (IsBuildNewFunction)
			{
				string DefaultFileData = "";

				for (int row = 0; row < curCSVFile.EditorElement.Length && row < 3; ++row)
				{
					for (int col = 0; col < curCSVFile.EditorElement[row].Length; ++col)
					{
						DefaultFileData += curCSVFile.EditorElement[row][col];

						if (col < curCSVFile.EditorElement[row].Length - 1)
							DefaultFileData += ",";
					}

					DefaultFileData += "\\r\\n";
				}

				FileReaderClass += "\r\n\tstatic string msDefaultFileData = \"";
				FileReaderClass += DefaultFileData;
				FileReaderClass += "\";\r\n";

				FileReaderClass += "\tpublic static CSVFile NewCSVFile()\r\n"
						+ "\t{\r\n"
						+ "\t\tCSVFile curCSVFile = CSVFile.CreateCSVFileFromMemory(msDefaultFileData);\r\n"
						+ "\t\tif (curCSVFile == null)\r\n"
						+ "\t\t\treturn null;\r\n"
						+ "\t\treturn curCSVFile;\r\n"
						+ "\t}\r\n";

				FileReaderClass += "\tpublic void SaveToCSV(string strPath)\r\n";

				FileReaderClass += "\t{\r\n\t\tCSVFile csvFile = CSVFile.CreateCSVFileByStream(strPath);\r\n"
								+ "\t\tif (csvFile == null)\r\n"
								+ "\t\t{\r\n"
								+ "\t\t\tcsvFile = CSVFile.CreateCSVFileFromMemory(msDefaultFileData);\r\n"
								+ "\t\t}\r\n";

				FileReaderClass += "\t\tstring[][] curElement = new string[DataInFile.Length + 3][];\r\n";

				FileReaderClass += "\t\r\n";
				FileReaderClass += "\t\tcurElement[0] = csvFile.EditorElement[0];\r\n";
				FileReaderClass += "\t\tcurElement[1] = csvFile.EditorElement[1];\r\n";
				FileReaderClass += "\t\tcurElement[2] = csvFile.EditorElement[2];\r\n";
				FileReaderClass += "\t\r\n";

				if (useKeyCol)
				{
					FileReaderClass += "\t\t" + DicnaryString + ".Enumerator Iter = DataInFile.GetEnumerator();\r\n";
					FileReaderClass += "\t\t\r\n";

					FileReaderClass += "\t\tfor (int row = 0; row < DataInFile.Length && Iter.MoveNext(); ++row)\r\n";
					FileReaderClass += "\t\t{\r\n";
					FileReaderClass += "\t\t\t" + FileDataClassName + " cur = Iter.Current.Value;\r\n";
					FileReaderClass += "\t\t\tcurElement[row + 3] = new string[" + curCSVFile.ColNum() + "];\r\n";
					
					for (int col = 0; col < curCSVFile.ColNum(); ++col)
					{
						string colName = curCSVFile.GetColName(col);
						switch (curCSVFile.GetColType(col))
						{
							case CSVType.BOOL:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = cur." + colName + "? \"1\" : \"0\";\r\n"; 
								break;
							case CSVType.VECTOR2:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = string.Format(\"{0} {1}\", cur." + colName + ".x, cur." + colName + ".y);\r\n"; 
								break;
							case CSVType.VECTOR3:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = string.Format(\"{0} {1} {2}\", cur." + colName + ".x, cur." + colName + ".y, cur." + colName + ".z);\r\n"; 
								break;
							case CSVType.STRING:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = cur." + curCSVFile.GetColName(col) + ";\r\n"; 
								break;
							default:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = cur." + curCSVFile.GetColName(col) + ".ToString();\r\n"; 
								break;
						}
					}
				}
				else
				{
					FileReaderClass += "\t\tfor (int row = 0; row < DataInFile.Length; ++row)\r\n";
					FileReaderClass += "\t\t{\r\n";
					FileReaderClass += "\t\t\t" + FileDataClassName + " cur = DataInFile[row];\r\n";
					FileReaderClass += "\t\t\tcurElement[row + 3] = new string[" + curCSVFile.ColNum() + "];\r\n";

					for (int col = 0; col < curCSVFile.ColNum(); ++col)
					{
						string colName = curCSVFile.GetColName(col);
						switch (curCSVFile.GetColType(0))
						{
							case CSVType.BOOL:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = cur." + colName + "? \"1\" : \"0\";\r\n";
								break;
							case CSVType.VECTOR2:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = string.Format(\"{0} {1}\", cur." + colName + ".x, cur." + colName + ".y);\r\n";
								break;
							case CSVType.VECTOR3:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = string.Format(\"{0} {1} {2}\", cur." + colName + ".x, cur." + colName + ".y, cur." + colName + ".z);\r\n";
								break;
							case CSVType.STRING:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = cur." + curCSVFile.GetColName(col) + ";\r\n";
								break;
							default:
								FileReaderClass += "\t\t\tcurElement[row + 3][" + col + "] = cur." + curCSVFile.GetColName(col) + ".ToString();\r\n";
								break;
						}
					}

					
				}

				FileReaderClass += "\t\t}\r\n";
				FileReaderClass += "\t\tcsvFile.EditorElement = curElement;\r\n";
				FileReaderClass += "\t\tcsvFile.Save(strPath);\r\n";
				FileReaderClass += "\t}\r\n";
			}
			#endregion

			FileReaderClass += "};\r\n";

			m_CSFileData += FileDataClass;
			m_CSFileData += FileReaderClass;

			return true;
		}

		public static void CreateCSFile(string FileName)
		{
			string folderPath = FileName.Substring(0, FileName.LastIndexOf('/'));

			if (folderPath.Equals(string.Empty))
			{
				return;
			}

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);

				AssetDatabase.Refresh();
			}

			FileStream file = new FileStream(FileName, FileMode.Create);

			// 编码转换貌似依旧无法解决乱码问题 乱码解决unity又不认识这代码
			// 暂时无解 直接强转
			byte[] byteArray = System.Text.Encoding.Convert(
										System.Text.Encoding.UTF8,
										System.Text.Encoding.ASCII,
										System.Text.Encoding.UTF8.GetBytes(m_CSFileData));
			//System.Text.Encoding.UTF8.GetBytes (  m_CSFileData);

			file.Write(byteArray, 0, byteArray.Length);
			file.Close();
			AssetDatabase.Refresh();
		}

	}
}
#endif