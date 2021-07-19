using System;
using System.Collections.Generic;
using Frameworks.Common;

using System.IO;
using ExcelDataReader;
using System.Text;
using System.Data;

namespace Frameworks.Json
{
	static class ExcelToCSVConverter
	{
		public static bool Convert(string excelPath, string csvPath)
		{
			if (string.IsNullOrEmpty(excelPath)
				|| string.IsNullOrEmpty(csvPath))
			{
				Log.Print(LogLevel.Error, "ConvertExcelToCSV Error! excelPath == {0}, csvPath == {1}", excelPath, csvPath);
				return false;
			}

			FileStream			excelFile = null;
			IExcelDataReader	readerData = null;
			DataTable			firstTable = null;

			try
			{
				excelFile = File.Open(excelPath, FileMode.Open, FileAccess.Read);
				readerData = ExcelReaderFactory.CreateOpenXmlReader(excelFile);
				firstTable = readerData.AsDataSet().Tables[0];
			}
			catch(Exception ex)
			{
				if (excelFile != null)
					excelFile.Close();
				Log.Print(LogLevel.Error, "ConvertExcelToCSV Error! open excel file failed! excelPath == {0}, exception: {1}", excelPath, ex.ToString());
				return false;
			}

			int columns = firstTable.Columns.Count;
			int rows	= firstTable.Rows.Count;

			using (StreamWriter csvWriter = new StreamWriter(csvPath, false, Encoding.UTF8))
			{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						string csvData = firstTable.Rows[i][j].ToString();

						if ( csvData.Contains(",")
							|| csvData.Contains("\n")
							|| csvData.Contains("\t") )
						{
							csvData = string.Format("\"{0}\"", csvData);
						}

						csvWriter.Write(csvData);

						if (j != columns-1)
							csvWriter.Write(',');
						else
							csvWriter.Write("\r\n");
					}
				}
			}

			excelFile.Close();

			return true;
		}
	}
}
