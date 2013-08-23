using System;
using NUnit.Framework;

namespace Electrolyte.Test {
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
	}
}

