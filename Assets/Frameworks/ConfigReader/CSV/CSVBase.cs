using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


namespace Frameworks.CSV
{
	public class CSVBase
	{
		public class CSVLineData
		{
			public List<string> colData = new List<string>();
		}

		CSVBase()
		{

		}

		public List<CSVLineData> LineData = null;

		public static CSVBase CreateCSV(string csvTextData)
		{
			char[] colSplit = new char[] { ',', '\t', '\n' };

			CSVBase csv = new CSVBase();

			csv.LineData = new List<CSVLineData>();

			CSVLineData data = new CSVLineData();
			for (int i = 0; i < csvTextData.Length; ++i)
			{
				if (csvTextData[i] == '"')
				{
					int nextQuotationIndex = csvTextData.IndexOf('"', i + 1);

					data.colData.Add(csvTextData.Substring(i + 1, nextQuotationIndex - i - 1));

					i = nextQuotationIndex + 1;

					if (i >= csvTextData.Length)
						break;

					if (csvTextData[i] == '\r')
					{
						++i;
						csv.LineData.Add(data);
						if (i != csvTextData.Length - 1)
							data = new CSVLineData();
					}

					continue;
				}

				int nextComma = csvTextData.IndexOfAny(colSplit, i);

				if (nextComma == -1)
				{
					//Debug.LogErrorFormat("CSV Format Error! text index == {0}", i);
					data.colData.Add(csvTextData.Substring(i, csvTextData.Length - i));
					csv.LineData.Add(data);
					break;
				}


				if (csvTextData[nextComma] != '\n' && nextComma > 0)
					data.colData.Add(csvTextData.Substring(i, nextComma - i));
				else
					data.colData.Add(csvTextData.Substring(i, nextComma - i - 1));

				i = nextComma;

				if (csvTextData[nextComma] == '\n' || nextComma == csvTextData.Length - 1)
				{
					csv.LineData.Add(data);

					if (nextComma != csvTextData.Length - 1)
						data = new CSVLineData();
				}
			}

			return csv;
		}
	
		public override string ToString()
		{
			string rt = "";

			char[] specialChar = new char[] { '"', '\n', ',', '\t'};

			for (int row = 0; row < LineData.Count; ++row)
			{
				var colData = LineData[row].colData;
				for (int col = 0; col < colData.Count; ++col)
				{
					if (colData[col].IndexOfAny(specialChar) == -1)
						rt += colData[col];
					else
						rt += string.Format("\"{0}\"", colData[col]);

					if (col != colData.Count - 1)
						rt += ",";
				}

				if (row != LineData.Count -1)
					rt += "\r\n";
			}

			return rt;
		}
	}
}
