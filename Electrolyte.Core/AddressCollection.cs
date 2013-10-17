using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Electrolyte.Extensions;

namespace Electrolyte {
	public class AddressCollection : IList<AddressDetails>, IDictionary<Address, AddressDetails> {
		List<Address> Addresses;
		List<AddressDetails> Details;

		public ICollection<Address> Keys {
			get { return Addresses.AsReadOnly(); }
		}

		public ICollection<AddressDetails> Values {
			get { return Details.AsReadOnly(); }
		}

		Dictionary<Address, AddressDetails> Dictionary {
			get { return Details.ToDictionary(d => d.Address, d => d); }
		}

		public int Count {
			get { return Details.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}



		public AddressCollection() {
			Addresses = new List<Address>();
			Details = new List<AddressDetails>();
		}

		public AddressDetails this[int i] {
			get { return Details[i]; }
			set {
				Addresses[i] = value.Address;
				Details[i] = value;
			}
		}

		public AddressDetails this[Address address] {
			get {
				return Details[Addresses.IndexOf(address)];
			}

			set {
				int i = Addresses.IndexOf(address);
				Addresses[i] = value.Address;
				Details[i] = value;
			}
		}

		public int IndexOf(AddressDetails details) {
			return Details.IndexOf(details);
		}

		public int IndexOf(Address address) {
			return Addresses.IndexOf(address);
		}

		public void Insert(int i, AddressDetails details) {
			Details.Insert(i, details);
			Addresses.Insert(i, details.Address);
		}

		public void Insert(int i, Address address, string label = null) {
			Insert(i, new AddressDetails(address, label));
		}

		public void Insert(int i, string address, string label = null) {
			Insert(i, new Address(address), label);
		}

		public void RemoveAt(int i) {
			Details.RemoveAt(i);
			Addresses.RemoveAt(i);
		}

		public void Add(AddressDetails details) {
			Details.Add(details);
			Addresses.Add(details.Address);
		}

		public void Add(Address address, AddressDetails details) {
			if(details.Address != address)
				throw new ArgumentException("Provided key did not match details", "address");
			Add(details);
		}

		public void Add(Address address, string label = null) {
			Add(new AddressDetails(address, label));
		}

		public void Add(string address, string label = null) {
			Add(new Address(address), label);
		}

		public void Add(KeyValuePair<Address, AddressDetails> pair) {
			Add(pair.Key, pair.Value);
		}

		public bool Remove(Address address) {
			try {
				RemoveAt(Addresses.IndexOf(address));
				return true;
			}
			catch {
				return false;
			}
		}

		public bool Remove(AddressDetails details) {
			try {
				RemoveAt(Details.IndexOf(details));
				return true;
			}
			catch {
				return false;
			}
		}

		public bool Remove(KeyValuePair<Address, AddressDetails> pair) {
			try {
				int i = Addresses.IndexOf(pair.Key);
				if(Addresses[i] == pair.Key && Details[i] == pair.Value) {
					RemoveAt(i);
					return true;
				}
				return false;
			}
			catch {
				return false;
			}
		}

		public bool ContainsKey(Address address) {
			return Addresses.Contains(address);
		}

		public bool Contains(AddressDetails details) {
			return Details.Contains(details);
		}

		public bool Contains(KeyValuePair<Address, AddressDetails> pair) {
			return ContainsKey(pair.Key) && Details[Addresses.IndexOf(pair.Key)] == pair.Value;
		}

		public bool TryGetValue(Address address, out AddressDetails details) {
			try {
				details = this[address];
				return true;
			}
			catch {
				details = default(AddressDetails);
				return false;
			}
		}

		public void CopyTo(AddressDetails[] details, int index) {
			Details.CopyTo(details, index);
		}

		public void CopyTo(KeyValuePair<Address, AddressDetails>[] pairs, int index) {
			Dictionary.Select(p => p).ToList().CopyTo(pairs, index);
		}

		public void Clear() {
			Addresses.Clear();
			Details.Clear();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return Details.GetEnumerator();
		}

		IEnumerator<AddressDetails> IEnumerable<AddressDetails>.GetEnumerator() {
			return Details.GetEnumerator();
		}

		IEnumerator<KeyValuePair<Address, AddressDetails>> IEnumerable<KeyValuePair<Address, AddressDetails>>.GetEnumerator() {
			return Dictionary.GetEnumerator();
		}

//		IEnumerator IEnumerable<AddressDetails>.GetEnumerator() {
//			return Details.GetEnumerator();
//		}

//		IEnumerator IEnumerable<AddressDetails>.GetEnumerator() {
//			return Details.GetEnumerator();
//		}
	}
}

