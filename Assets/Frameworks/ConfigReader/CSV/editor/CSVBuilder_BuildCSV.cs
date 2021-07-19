using UnityEngine;
using System;
using System.Collections.Generic;

namespace Frameworks.CSV
{
	public partial class CSVBuilderReader
	{
		public static string defaultCSVFileData = "CSVName,CSVPath,CSPath,IsUseKey,KeyNum,IsBuildResourceCreateFunction,IsBuildMemoryCreateFunction,IsBuildNewFunction\r\nThe name of csv.,Csv file path.,Build .cs file path.,,,,,\r\nString,String,String,Bool,Int,Bool,Bool,Bool\r\n";

		public void SaveToCSV(string strPath)
		{

			CSVFile csvFile = CSVFile.CreateCSVFileByStream(strPath);

			if (csvFile == null)
			{
				csvFile = CSVFile.CreateCSVFileFromMemory(defaultCSVFileData);
			}


			string[][] curElement = new string[DataInFile.Count + 3][];

			curElement[0] = csvFile.EditorElement[0];
			curElement[1] = csvFile.EditorElement[1];
			curElement[2] = csvFile.EditorElement[2];

			Dictionary<string, CSVBuilderData>.Enumerator Iter = DataInFile.GetEnumerator();
			for (int row = 0; row < DataInFile.Count && Iter.MoveNext(); ++row)
			{
				CSVBuilderData cur = Iter.Current.Value;
				curElement[row + 3] = new string[7];
				curElement[row + 3][0] = cur.CSVName;
				curElement[row + 3][1] = cur.CSVPath;
				curElement[row + 3][2] = cur.CSPath;
				curElement[row + 3][3] = cur.IsUseKey ? "1" : "0";
				curElement[row + 3][4] = cur.KeyNum.ToString();
				curElement[row + 3][5] = cur.IsBuildResourceCreateFunction ? "1" : "0";
				curElement[row + 3][6] = cur.IsBuildMemoryCreateFunction ? "1" : "0";
			}

			csvFile.EditorElement = curElement;

			csvFile.Save(strPath);
		}
	}

}