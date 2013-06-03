using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Electrolyte {
	[Serializable]
	public class InvalidAddressException : Exception { }

	public class Address {
		protected static Regex Pattern = new Regex(@"\A^[13][1-9A-Za-z][^OIl]{20,40}\Z");

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

