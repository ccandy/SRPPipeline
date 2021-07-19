using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Frameworks.Common
{
	public enum LogType
	{
		ToLog,
		ToBuffer,
	}

	public enum LogLevel
	{
		Info,
		Warning,
		Error,
	}

	public class Log
	{
		public static int MaxBufferLogNum = 20;
		public static LinkedList<string> LogBufferList = null;
		public static bool HasNewLogInBuffer = false;

		public static LogType msType = LogType.ToLog;

#if _MUL_THREAD_
	static System.Object LockObject = new System.Object();
#endif

		static public void PrintToLog(LogLevel lev, string log)
		{
#if NO_LOG
			return;
#endif
#if _MUL_THREAD_
		
			lock(LockObject)
			{
#endif
			switch (lev)
			{
				case LogLevel.Error:
					Debug.LogError(log);
					break;
				case LogLevel.Warning:
					Debug.LogWarning(log);
					break;
				case LogLevel.Info:
					Debug.Log(log);
					break;
			}
#if _MUL_THREAD_
			}
#endif
		}

		public static string GetBufferLogTotal()
		{
			if (LogBufferList == null)
				return "";

			//System.Text.StringBuilder builder = new System.Text.StringBuilder(4096);

			//LinkedListNode<string> node = LogBufferList.First;
			//while (node != null)
			//{
			//	builder.Append(node.Value);
			//	node = node.Next;
			//}

			//HasNewLogInBuffer = false;

			//return builder.ToString(0, builder.Length);

			return string.Concat(LogBufferList);
		}

		public static void PrintToBuffer(LogLevel eFlag, string Info)
		{
#if _NO_LOG_
			return;
#endif

			if (LogBufferList == null)
				return;

#if _MUL_THREAD_

			lock (LockObject)
			{
#endif
			string NewLog = "";
			switch (eFlag)
			{
				case LogLevel.Error:
					NewLog += "Error:";
					break;
				case LogLevel.Warning:
					NewLog += "Warning:";
					break;
				case LogLevel.Info:
					NewLog += "Info:";
					break;
			}

			NewLog += Info + "\n";

			LogBufferList.AddLast(NewLog);
			HasNewLogInBuffer = true;

			if (LogBufferList.Count > MaxBufferLogNum)
			{
				LogBufferList.RemoveFirst();
			}
#if _MUL_THREAD_
			}
#endif
		}

		public static void Print(LogLevel eFlag, string log)
		{
			switch (msType)
			{
				case LogType.ToLog:
					PrintToLog(eFlag, log);
					break;
				case LogType.ToBuffer:
					PrintToBuffer(eFlag, log);
					PrintToLog(eFlag, log);
					break;
			}

		}

		public static void Print(LogLevel lev, string format, params object[] args)
		{
			string LogInfo = string.Format(format, args);
			Print(lev, LogInfo);
		}
	}
}
