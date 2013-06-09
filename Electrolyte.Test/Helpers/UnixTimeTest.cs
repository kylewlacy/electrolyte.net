using System;
using NUnit.Framework;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Helpers {
	[TestFixture]
	public class UnixTimeTest {
		[Test]
		public void UnixTimeToDateTime() {
			DateTime original = new DateTime(2012, 12, 19, 06, 12, 33, DateTimeKind.Utc);
			DateTime conversion = UnixTime.DateTimeFromUnixTime(1355897553);

			Assert.AreEqual(conversion, original);
		}

		[Test]
		public void DateTimeToUnixTime() {
			long original = 1370596465;
			long conversion = UnixTime.UnixTimeFromDateTime(new DateTime(2013, 06, 07, 02, 14, 25));

			Assert.AreEqual(conversion, original);
		}
	}
}

