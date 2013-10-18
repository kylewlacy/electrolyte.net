using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Electrolyte.Extensions;

namespace Electrolyte {
	public class AddressCollection : IList<AddressDetails> {
		List<Address> _addresses;
		List<AddressDetails> _details;

		public ICollection<Address> Addresses {
			get { return _addresses.AsReadOnly(); }
		}

		public ICollection<AddressDetails> AddressDetails {
			get { return _details.AsReadOnly(); }
		}

		public int Count {
			get { return _details.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}



		public AddressCollection(IEnumerable<AddressDetails> addresses) {
			_addresses = addresses.Select(a => a.Address).ToList();
			_details = addresses.ToList();
		}

		public AddressCollection(IEnumerable<Address> addresses) {
			_addresses = addresses.ToList();
			_details = addresses.Select(a => new AddressDetails(a)).ToList();
		}

		public AddressCollection() {
			_addresses = new List<Address>();
			_details = new List<AddressDetails>();
		}

		public AddressDetails this[int i] {
			get { return _details[i]; }
			set {
				_addresses[i] = value.Address;
				_details[i] = value;
			}
		}

		public AddressDetails this[Address address] {
			get {
				return _details[_addresses.IndexOf(address)];
			}

			set {
				int i = _addresses.IndexOf(address);
				_addresses[i] = value.Address;
				_details[i] = value;
			}
		}

		public int IndexOf(AddressDetails details) {
			return _details.IndexOf(details);
		}

		public int IndexOf(Address address) {
			return _addresses.IndexOf(address);
		}

		public void Insert(int i, AddressDetails details) {
			_details.Insert(i, details);
			_addresses.Insert(i, details.Address);
		}

		public void Insert(int i, Address address, string label = null) {
			Insert(i, new AddressDetails(address, label));
		}

		public void Insert(int i, string address, string label = null) {
			Insert(i, new Address(address), label);
		}

		public void RemoveAt(int i) {
			_details.RemoveAt(i);
			_addresses.RemoveAt(i);
		}

		public void Add(AddressDetails details) {
			_details.Add(details);
			_addresses.Add(details.Address);
		}

		public void Add(Address address, string label = null) {
			Add(new AddressDetails(address, label));
		}

		public void Add(string address, string label = null) {
			Add(new Address(address), label);
		}

		public bool Remove(Address address) {
			try {
				RemoveAt(_addresses.IndexOf(address));
				return true;
			}
			catch {
				return false;
			}
		}

		public bool Remove(AddressDetails details) {
			try {
				RemoveAt(_details.IndexOf(details));
				return true;
			}
			catch {
				return false;
			}
		}

		public bool Contains(Address address) {
			return _addresses.Contains(address);
		}

		public bool Contains(AddressDetails details) {
			return _details.Contains(details);
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
			_details.CopyTo(details, index);
		}

		public void Clear() {
			_addresses.Clear();
			_details.Clear();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _details.GetEnumerator();
		}

		IEnumerator<AddressDetails> IEnumerable<AddressDetails>.GetEnumerator() {
			return _details.GetEnumerator();
		}

		public List<AddressDetails> ToList() {
			return _details.ToList();
		}

		public Dictionary<Address, AddressDetails> ToDictionary() {
			return _details.ToDictionary(d => d.Address, d => d);
		}
	}
}

