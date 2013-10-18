using System;
using System.Collections.Generic;

namespace Electrolyte {
	public class AddressDetails : IEquatable<AddressDetails> {
		public virtual Address Address { get; set; }
		public virtual string Label { get; set; }

		protected AddressDetails() { }
		public AddressDetails(string address, string label = null) : this(new Address(address), label) { }
		public AddressDetails(Address address, string label = null) {
			Address = address;
			Label = label;
		}

		public AddressDetails(AddressDetails details) : this(details.Address, details.Label) { }

		public override string ToString() {
			return String.Format(String.IsNullOrEmpty(Label) ? "{0}" : "{0} - {1}", Address, Label);
		}

		public override bool Equals(object obj) {
			if(obj is AddressDetails) {
				return Equals((AddressDetails)obj);
			}
			return false;
		}

		public bool Equals(AddressDetails other) {
			return other.Address == Address && other.Label == Label;
		}

		public override int GetHashCode() {
			return Address.GetHashCode();
		}

		public static bool operator ==(AddressDetails a, AddressDetails b) {
			return (object)a != null && (object)b != null && a.Equals(b);
		}

		public static bool operator !=(AddressDetails a, AddressDetails b) {
			return (object)a == null || (object)b == null || !(a == b);
		}
	}
}

