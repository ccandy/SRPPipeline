using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frameworks.KVFile
{
	public class KVFile
	{
		public Dictionary<string, string> KVData = new Dictionary<string, string>();

		public override string ToString()
		{
			string rt = "";

			foreach (var pair in KVData)
			{
				rt += string.Format("{0}={1}\r\n", pair.Key, pair.Value);
			}

			return rt;
		}

		public void ParseData(string str)
		{
			KVData.Clear();
			AddParseData(str);
		}

		public void AddParseData(string str)
		{
			string[] KVStrData = str.Replace("\r\n", "\n").Split('\n');

			for (int i = 0; i < KVStrData.Length; ++i)
			{
				var KV = KVStrData[i].Split('=');
				if (KV.Length != 2)
					continue;

				KVData[KV[0]] = KV[1];
			}
		}
	}
}
