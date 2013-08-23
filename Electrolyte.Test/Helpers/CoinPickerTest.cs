using System;
using NUnit.Framework;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Helpers {
	[TestFixture]
	public class CoinPickerTest {
		[Test]
		public void CalculateFee() {
			Assert.AreEqual(Money.Zero("BTC"), CoinPicker.FeeForTx(0));
			Assert.AreEqual(Money.Create(0.0001m, "BTC"), CoinPicker.FeeForTx(500));
			Assert.AreEqual(Money.Create(0.0001m, "BTC"), CoinPicker.FeeForTx(1000));
			Assert.AreEqual(Money.Create(0.0002m, "BTC"), CoinPicker.FeeForTx(1001));
		}

		[Test]
		[Ignore("How do we write a good test for estimating something?")]
		public void EstimateFee() {

		}
	}
}

