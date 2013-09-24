using System;
using NUnit.Framework;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Helpers {
	[TestFixture]
	public class MoneyTest {
		[Test]
		public void MoneyComparisions() {
			Money m1 = Money.Create(1.00m, "USD");
			Money m2 = Money.Create(1.00m, "USD");
			Money m3 = Money.Create(1.50m, "USD");
			Money m4 = Money.Create(0.50m, "USD");
			
			Assert.IsTrue(m1 == m1);
			Assert.IsTrue(m1 == m2);
			Assert.IsFalse(m1 == m3);
			Assert.IsFalse(m1 == m4);
			Assert.IsTrue(m1.Equals(m1));
			Assert.IsTrue(m1.Equals(m2));
			Assert.IsFalse(m1.Equals(m3));
			Assert.IsFalse(m1.Equals(m4));

			Assert.IsFalse(m1 > m1);
			Assert.IsFalse(m1 > m2);
			Assert.IsFalse(m1 > m3);
			Assert.IsTrue(m1 > m4);

			Assert.IsFalse(m1 < m1);
			Assert.IsFalse(m1 < m2);
			Assert.IsTrue(m1 < m3);
			Assert.IsFalse(m1 < m4);

			Assert.IsTrue(m1 >= m1);
			Assert.IsTrue(m1 >= m2);
			Assert.IsFalse(m1 >= m3);
			Assert.IsTrue(m1 >= m4);

			Assert.IsTrue(m1 <= m1);
			Assert.IsTrue(m1 <= m2);
			Assert.IsTrue(m1 <= m3);
			Assert.IsFalse(m1 <= m4);
		}

		[Test]
		public void MoneyOperations() {
			Assert.AreEqual(Money.Create(1m, "USD") + Money.Create(0.5m, "USD"), Money.Create(1.5m, "USD"));
			Assert.AreEqual(Money.Create(1m, "USD") - Money.Create(0.5m, "USD"), Money.Create(0.5m, "USD"));
			Assert.AreEqual(Money.Create(1m, "USD") * 2, Money.Create(2m, "USD"));
			Assert.AreEqual(Money.Create(1m, "USD") * 0.5m, Money.Create(0.5m, "USD"));
			Assert.AreEqual(Money.Create(1m, "USD") / 2, Money.Create(0.5m, "USD"));
			Assert.AreEqual(Money.Create(1m, "USD") / 0.5m, Money.Create(2m, "USD"));
		}

		[Test]
		public void MoneyConversion() {
			Assert.Throws<Money.UnknownCurrencyException>(() => {
				Money.Create(1.00m, "ABC");
			});

			Money.CurrencyType.Register("ABC", "ABC", 1000);

			Assert.Throws<Money.IncompatibleCurrencyException>(() => {
				Money money = Money.Create(1.00m, "USD") + Money.Create(1.00m, "ABC");
			});

			Assert.Throws<Money.UnknownExchangeRateException>(() => {
				(Money.Create(10m, "USD")).ExchangeTo("ABC");
			});

			Money.VariableBank bank = new Money.VariableBank();
			Money.CurrentBank = bank;

			bank.AddRate(from: "ABC", to: "USD", rate: 10.00m);

			Assert.AreEqual(Money.Create(10m, "USD"), Money.Create(1m, "ABC").ExchangeTo("USD"));
			Assert.AreEqual(Money.Create(10m, "ABC"), Money.Create(100m, "USD").ExchangeTo("ABC"));

			bank.AddRate(from: "USD", to: "ABC", rate: 10.00m);

			Assert.AreEqual(Money.Create(10m, "USD"), Money.Create(100m, "ABC").ExchangeTo("USD"));
			Assert.AreEqual(Money.Create(10m, "ABC"), Money.Create(1m, "USD").ExchangeTo("ABC"));
		}

		[Test]
		[Ignore("How should values be rounded?")]
		public void Rounding() {

		}

		[Test]
		new public void ToString() {
			Money.CurrencyType.Register("DEF", "DEF", 1000);
			Assert.AreEqual("1.000 DEF", Money.Create(1.000m, "DEF").ToString());
			Assert.AreEqual("12.345 DEF", Money.Create(12.345m, "DEF").ToString());
			Assert.AreEqual("0.001 DEF", Money.Create(0.001m, "DEF").ToString());
			Assert.AreEqual("0.000 DEF", Money.Zero("DEF").ToString());

			
			Money.CurrencyType.Register("GHI", "GHI", 2500);
			Assert.AreEqual("1.000 GHI", Money.Create(1.0000m, "GHI").ToString());
			Assert.AreEqual("12.345 GHI", Money.Create(12.3450m, "GHI").ToString());
			Assert.AreEqual("0.000 GHI", Money.Zero("GHI").ToString());

			Assert.AreEqual("$1.00", Money.Create(1.00m, "USD").ToString(true));
			Assert.AreEqual("$12.34", Money.Create(12.34m, "USD").ToString(true));
			Assert.AreEqual("$0.01", Money.Create(0.01m, "USD").ToString(true));
			Assert.AreEqual("$0.00", Money.Zero("USD").ToString(true));
		}
	}
}

