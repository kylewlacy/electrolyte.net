using System;
using NUnit.Framework;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Helpers {
	[TestFixture]
	public class UnixTimeTest {
		Tuple<int, DateTime>[] Times = new Tuple<int, DateTime>[] {
			Tuple.Create(+1355897553, new DateTime(2012, 12, 19, 06, 12, 33, DateTimeKind.Utc)),
			Tuple.Create(+0000000000, new DateTime(1970, 01, 01, 00, 00, 00, DateTimeKind.Utc)),
			Tuple.Create(+1370821080, new DateTime(2013, 06, 09, 23, 38, 00, DateTimeKind.Utc)),
			Tuple.Create(-0014182940, new DateTime(1969, 07, 20, 20, 17, 40, DateTimeKind.Utc))
		};

		[Test]
		public void UnixTimeToDateTime() {
			foreach(var time in Times) {
				Assert.AreEqual(UnixTime.DateTimeFromUnixTime(time.Item1), time.Item2);
			}
		}

		[Test]
		public void DateTimeToUnixTime() {
			foreach(Tuple<int, DateTime> time in Times) {
				Assert.AreEqual(UnixTime.UnixTimeFromDateTime(time.Item2), time.Item1);
			}
		}
	}
}

