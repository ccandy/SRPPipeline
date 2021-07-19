using UnityEngine;
using System;
using System.Collections.Generic;

namespace Frameworks.CSV
{
	public partial class CSVBuilderData
	{
		public string CSVName;
		public string CSVPath;
		public string CSPath;
		public bool IsUseKey;
		public int KeyNum;
		public bool IsBuildResourceCreateFunction;
		public bool IsBuildMemoryCreateFunction;
		public bool IsBuildNewFunction;
	};
	public partial class CSVBuilderReader
	{
		public Dictionary<string, CSVBuilderData> DataInFile;
		public static CSVBuilderReader Create(CSVFile curCSVFile)
		{
			CSVBuilderReader curReader = null;
			if (curCSVFile == null)
				return curReader;
			curReader = new CSVBuilderReader();
			int rowNum = curCSVFile.RowNum();
			if (rowNum == 0)
				return curReader;
			curReader.DataInFile = new Dictionary<string, CSVBuilderData>();
			for (int row = 0; row < rowNum; ++row)
			{
				string curKey = curCSVFile.GetData(row, 0);
				if (curReader.DataInFile.ContainsKey(curKey))
				{
					continue;
				}
				CSVBuilderData curData = new CSVBuilderData();
				curData.CSVName = curCSVFile.GetData(row, 0);
				curData.CSVPath = curCSVFile.GetData(row, 1);
				curData.CSPath = curCSVFile.GetData(row, 2);
				curData.IsUseKey = curCSVFile.GetBoolData(row, 3);
				curData.KeyNum = curCSVFile.GetIntData(row, 4);
				curData.IsBuildResourceCreateFunction = curCSVFile.GetBoolData(row, 5);
				curData.IsBuildMemoryCreateFunction = curCSVFile.GetBoolData(row, 6);
				curData.IsBuildNewFunction = curCSVFile.GetBoolData(row, 7);
				curReader.DataInFile.Add(curKey, curData);
			}

			return curReader;
		}
		public static CSVBuilderReader CreateFromMemory(byte[] data)
		{
			CSVFile curCSVFile = CSVFile.CreateCSVFileFromMemory(data);
			if (curCSVFile == null)
				return null;
			return Create(curCSVFile);
		}
	};

}
