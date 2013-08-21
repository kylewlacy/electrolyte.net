using System;
using NUnit.Framework;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Helpers {
	[TestFixture]
	public class CoinPickerTest {
		[Test]
		public void CalculateFee() {
			Assert.AreEqual(0, CoinPicker.FeeForTx(0));
			Assert.AreEqual(10000, CoinPicker.FeeForTx(500));
			Assert.AreEqual(10000, CoinPicker.FeeForTx(1000));
			Assert.AreEqual(20000, CoinPicker.FeeForTx(1001));
		}

		[Test]
		[Ignore("How do we write a good test for estimating something?")]
		public void EstimateFee() {

		}
	}
}

