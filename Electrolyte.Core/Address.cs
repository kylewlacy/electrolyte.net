using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Electrolyte {
	[Serializable]
	public class InvalidAddressException : Exception { }

	public class Address {
		// HACK: Magic numbers (should be {27,34}
		protected static Regex Pattern = new Regex(@"\A^[13][1-9A-Za-z][^OIl]{20,32}\Z");

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

