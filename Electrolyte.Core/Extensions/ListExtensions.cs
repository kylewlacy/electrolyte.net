using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Electrolyte.Extensions {
	public static class ListExtensions {
		public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) {
			return new ReadOnlyCollection<T>(list);
		}
	}
}

