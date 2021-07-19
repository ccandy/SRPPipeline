using System;
using System.IO;
using System.Text;

namespace Frameworks.CSV
{
	class CSVToJson
	{
		public  static void AnsiToUtf81(string asniFilePath, string utf8FilePath)
		{
			FileStream fs = new FileStream(asniFilePath, FileMode.Open, FileAccess.Read);
			StreamReader sr = new StreamReader(fs, Encoding.ASCII);

			long begin = fs.Seek(0, SeekOrigin.Begin);
			long length = fs.Seek(0, SeekOrigin.End) - begin;
			fs.Seek(0, SeekOrigin.Begin);

			char[] asniChar = new char[length];

			sr.Read(asniChar, 0, (int)length);

			fs.Close();
			sr.Close();

			//string txtFile = Path.GetDirectoryName(asniFilePath) + "/" + Path.GetFileNameWithoutExtension(asniFilePath) + "_Copy.txt";
			FileStream fsw = new FileStream(utf8FilePath, FileMode.OpenOrCreate, FileAccess.Write);

			//另存为UTF-8
			StreamWriter sw = new StreamWriter(fsw, Encoding.UTF8);
			
			if (asniChar.Length > 0)
			{
				sw.Write( asniChar, 0, asniChar.Length);
			}

			sw.Close();

			fsw.Close();
		}

		public static void AnsiToUtf8(string rbFile)
		{

			byte[] asniBytes = File.ReadAllBytes(rbFile);

			Encoding fromEncoding = Encoding.GetEncoding("GBK");

			var utf8Bytes = Encoding.Convert(fromEncoding, Encoding.UTF8, asniBytes);
			//var utf8Bytes = Encoding.Convert(Encoding.BigEndianUnicode, Encoding.UTF8, utfBytes);

			try
			{
				File.WriteAllBytes(rbFile + "_utf8.txt", utf8Bytes);
			}
			catch (Exception)
			{

			}
		}

	}
}
