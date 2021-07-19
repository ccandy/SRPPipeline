using System;
using System.Collections.Generic;
using UnityEngine;

namespace Frameworks
{
	public class TimeTools
	{
		public readonly static DateTime Time1970_0101 = new DateTime(1970, 1, 1);
		public readonly static DateTime CurrentTimeZone1970_0101 = TimeZone.CurrentTimeZone.ToLocalTime(Time1970_0101);

		public static long BeginTime()
		{
			return TimeGetTime();
		}

		public static long PassTime(long beginMS)
		{
			return TimeGetTime() - beginMS;
		}

		public static long TimeGetTime()
		{
			return ConvertDateTimeToMs(System.DateTime.Now);
		}

		public static long ConvertDateTimeToMs(System.DateTime time)
		{
			return (long)((time - CurrentTimeZone1970_0101).TotalMilliseconds);
		}

		public static DateTime ConvertMsToDateTime(long timeStampMs)
		{
			var startTime = TimeZone.CurrentTimeZone.ToLocalTime(Time1970_0101);
			var dt = startTime.AddMilliseconds(timeStampMs);
			return dt;
		}
	}
}
