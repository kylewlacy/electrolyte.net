using System;
using System.Linq;
using System.Collections.Generic;

namespace Electrolyte {
	// Based on https://github.com/RubyMoney/money
	public class Money {
		public class OperationException : System.InvalidOperationException {
			public OperationException() { }
			public OperationException(string message) : base(message) { }
			public OperationException(string message, Exception inner) : base(message, inner) { }
		}
		
		public class IncompatibleCurrencyException : OperationException {
			public IncompatibleCurrencyException() { }
			public IncompatibleCurrencyException(string message) : base(message) { }
			public IncompatibleCurrencyException(string message, Exception inner) : base(message, inner) { }
		}

		public class UnknownExchangeRateException : OperationException {
			public UnknownExchangeRateException() { }
			public UnknownExchangeRateException(string message) : base(message) { }
			public UnknownExchangeRateException(string message, Exception inner) : base(message, inner) { }
		}

		public class UnknownCurrencyException : OperationException {
			public UnknownCurrencyException() { }
			public UnknownCurrencyException(string message) : base(message) { }
			public UnknownCurrencyException(string message, Exception inner) : base(message, inner) { }
		}

		public abstract class Bank {
			public virtual Money ExchangeWith(Money money, CurrencyType currency) {
				return Money.Create(money.Currency == currency ? money.Value : money.Value * GetRate(from: money.Currency, to: currency), currency);
			}

			public virtual decimal GetRate(string @from, string to) {
				return GetRate(CurrencyType.FindByCode(@from), CurrencyType.FindByCode(to));
			}

			public abstract decimal GetRate(CurrencyType @from, CurrencyType to);
		}

		public class VariableBank : Bank {
			public Dictionary<CurrencyType, Dictionary<CurrencyType, decimal>> ExchangeRates = new Dictionary<CurrencyType, Dictionary<CurrencyType, decimal>>();

			public VariableBank() { }

			public override decimal GetRate(CurrencyType @from, CurrencyType to) {
				if(ExchangeRates.ContainsKey(@from) && ExchangeRates[@from].ContainsKey(to))
					return ExchangeRates[@from][to];
				if(ExchangeRates.ContainsKey(to) && ExchangeRates[to].ContainsKey(@from))
					return 1m / ExchangeRates[to][@from];

				throw new UnknownExchangeRateException(String.Format("Exchange rate between '{0}' and '{1}' not known", @from.Code, to.Code));
			}

			public void AddRate(string @from, string to, decimal rate) {
				AddRate(CurrencyType.FindByCode(@from), CurrencyType.FindByCode(to), rate);
			}

			public void AddRate(CurrencyType @from, CurrencyType to, decimal rate) {
				if(ExchangeRates.ContainsKey(to)) {
					CurrencyType c = @from;
					@from = to;
					to = c;
					rate = 1 / rate;
				}

				if(!ExchangeRates.ContainsKey(@from))
					ExchangeRates.Add(@from, new Dictionary<CurrencyType, decimal>());

				if(ExchangeRates[@from].ContainsKey(to))
					ExchangeRates[@from][to] = rate;
				else
					ExchangeRates[@from].Add(@to, rate);
			}
		}

		public class CurrencyType {
			public static Dictionary<string, CurrencyType> RegisteredCurrencies = new Dictionary<string, CurrencyType>();
			public string Code;
			public string Symbol;
			public ulong CentsPerUnit;

			static CurrencyType() {
				Register(code: "USD", symbol: "$", centsPerUnit: 100);
				Register(code: "BTC", symbol: "BTC", centsPerUnit: 100000000);
			}

			CurrencyType(string code, string symbol, ulong centsPerUnit) {
				Code = code;
				Symbol = symbol;
				CentsPerUnit = centsPerUnit;
			}

			public override int GetHashCode() {
				return Code.GetHashCode();
			}

			public static CurrencyType FindByCode(string code) {
				if(!RegisteredCurrencies.ContainsKey(code))
					throw new UnknownCurrencyException(String.Format("Unknown currency: '{0}'", code));
				return RegisteredCurrencies[code];
			}

			public static CurrencyType FindBySymbol(string symbol) {
				CurrencyType[] currencies = RegisteredCurrencies.Values.Where(c => c.Symbol == symbol).ToArray();
				if(currencies.Length <= 0)
					throw new UnknownCurrencyException(String.Format("Unknown currency with symbol '{0}'", symbol));
				return currencies.First();
			}

			public static CurrencyType Register(string code, string symbol, ulong centsPerUnit) {
				CurrencyType currency = new CurrencyType(code, symbol, centsPerUnit);
				RegisteredCurrencies.Add(code, currency);
				return currency;
			}
		}

		public static CurrencyType DefaultCurrencyType = CurrencyType.FindByCode("BTC");
		public static Bank CurrentBank = new VariableBank();

		public CurrencyType Currency { get; private set; }
		public Int64 Cents;
		public decimal Value {
			get { return Cents / (decimal)Currency.CentsPerUnit; }
			set { Cents = (long)Math.Round(Currency.CentsPerUnit * value); }
		}

		public Money(Int64 cents, string code) : this(cents, CurrencyType.FindByCode(code)) { }
		public Money(Int64 cents, CurrencyType currency) {
			Cents = cents;
			Currency = currency;
		}



		public Money ExchangeTo(string code) {
			return ExchangeTo(CurrencyType.FindByCode(code));
		}

		public Money ExchangeTo(CurrencyType currency) {
			return CurrentBank.ExchangeWith(this, currency);
		}



		public override string ToString() {
			return ToString(false);
		}

		public string ToString(bool useSymbol) {
			// TODO: Refactor this for clarity
			// http://stackoverflow.com/a/4483960/1311454
			int decimals = (int)Math.Floor(Math.Log10(Currency.CentsPerUnit) + 1) - 1;
			string numberFormat = String.Join("", new string[] { "0", ".", new String('0', decimals) });

			return String.Format(useSymbol ? "{2}{0}" : "{0} {1}", Value.ToString(numberFormat), Currency.Code, Currency.Symbol);
		}

		public override bool Equals(object obj) {
			Money money = obj as Money;
			return money != null && money.Currency == Currency && money.Cents == Cents;
		}

		public override int GetHashCode() {
			return ToString().GetHashCode();
		}



		public static Money Zero(string code) {
			return Zero(CurrencyType.FindByCode(code));
		}

		public static Money Zero(CurrencyType currency) {
			return new Money(0, currency);
		}

		public static Money Create(decimal value, CurrencyType currency) {
			Money money = Money.Zero(currency);
			money.Value = value;
			return money;
		}

		public static Money Create(decimal value, string code) {
			return Create(value, CurrencyType.FindByCode(code));
		}



		public static Money operator +(Money m1, Money m2) {
			if(m1.Currency != m2.Currency)
				throw new IncompatibleCurrencyException();
			return new Money(m1.Cents + m2.Cents, m1.Currency);
		}

		public static Money operator -(Money m1, Money m2) {
			if(m1.Currency != m2.Currency)
				throw new IncompatibleCurrencyException();
			return new Money(m1.Cents - m2.Cents, m1.Currency);
		}

		public static Money operator *(Money money, long scalar) {
			return new Money(money.Cents * scalar, money.Currency);
		}

		public static Money operator *(Money money, decimal scalar) {
			return Money.Create(money.Value * scalar, money.Currency);
		}

		public static Money operator /(Money money, long scalar) {
			return Money.Create(money.Value / (decimal)scalar, money.Currency);
		}

		public static Money operator /(Money money, decimal scalar) {
			return Money.Create(money.Value / scalar, money.Currency);
		}

		public static bool operator ==(Money m1, Money m2) {
			return (object)m1 != null && (object)m2 != null && m1.ExchangeTo(m2.Currency).Equals(m2);
		}

		public static bool operator !=(Money m1, Money m2) {
			return !(m1 == m2);
		}

		public static bool operator >(Money m1, Money m2) {
			return m1.ExchangeTo(m2.Currency).Cents > m2.Cents;
		}

		public static bool operator <(Money m1, Money m2) {
			return m2 > m1;
		}

		public static bool operator >=(Money m1, Money m2) {
			return !(m1 < m2);
		}

		public static bool operator <=(Money m1, Money m2) {
			return m2 >= m1;
		}
	}
}

