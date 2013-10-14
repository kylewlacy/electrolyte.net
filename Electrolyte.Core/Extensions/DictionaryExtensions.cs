using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Electrolyte.Extensions {
	public static class DictionaryExtensions {
		// http://stackoverflow.com/a/678404/1311454
		public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) {
			return new ReadOnlyDictionary<TKey, TValue>(dictionary);
		}
	}
}

