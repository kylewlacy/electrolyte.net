using System;
using System.Collections.Generic;
using System.Linq;
using Electrolyte.Helpers;

namespace Electrolyte.Extensions {
	public static class IEnumerableExtensions {
		// From http://theburningmonk.com/2010/11/extension-methods-to-sum-ienumerable-ulong-and-ienumerable-nullable-ulong/
		public static ulong Sum(this IEnumerable<ulong> source) {
			return source.Aggregate(0UL, (a, b) => a + b);
		}

		public static long Sum(this IEnumerable<long> source) {
			return source.Aggregate(0L, (a, b) => a + b);
		}

		public static Money Sum(this IEnumerable<Money> source) {
			Money.CurrencyType currency = source.Any() ? source.First().Currency : Money.DefaultCurrencyType;
			return source.Aggregate(new Money(0, currency), (a, b) => a + b);
		}
	}
}

