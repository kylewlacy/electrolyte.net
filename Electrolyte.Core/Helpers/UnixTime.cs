using System;

namespace Electrolyte.Helpers {
	public static class UnixTime {
		public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime DateTimeFromUnixTime(Int64 unixTime) {
			DateTime newTime = Epoch;
			return newTime.AddSeconds(unixTime);
		}

		public static Int64 UnixTimeFromDateTime(DateTime time) {
			return (Int64)(time.ToUniversalTime() - Epoch).TotalSeconds;
		}
	}
}

