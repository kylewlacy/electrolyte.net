using System;
using System.Collections.Generic;
using System.Linq;

namespace Electrolyte.Extensions {
	public static class IEnumerableExtensions {
		// From http://theburningmonk.com/2010/11/extension-methods-to-sum-ienumerable-ulong-and-ienumerable-nullable-ulong/
		public static ulong Sum(this IEnumerable<ulong> source) {
			UInt64 sum = 0UL;
			foreach(UInt64 number in source) {
				sum += number;
			}
			return sum;
		}
	}
}

