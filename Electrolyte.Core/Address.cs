using System;
using System.Text.RegularExpressions;

namespace Electrolyte {
	[Serializable]
	public class InvalidAddressException : Exception { }

	public class Address {
		// HACK: Magic numbers (should be {27,34}
		protected static Regex Pattern = new Regex(@"^[13][1-9A-Za-z][^OIl]{27,32}$");

		private string _id;
		public string ID {
			get {
				return _id;
			}
			set {
				if(!Pattern.IsMatch(value))
					throw new InvalidAddressException();
				_id = value;
			}
		}

		public Address(string id) {
			ID = id;
		}
	}
}

