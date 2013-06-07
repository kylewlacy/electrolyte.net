using System;

namespace Electrolyte.Helpers {
	public static class UnixTimeHelpers {
		public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime DateTimeFromUnixTime(Int64 unixTime) {
			return Epoch.AddSeconds(unixTime).ToLocalTime();
		}

		public static Int64 UnixTimeFromDateTime(DateTime time) {
			return (Int64)(time - Epoch).TotalSeconds;
		}
	}
}

