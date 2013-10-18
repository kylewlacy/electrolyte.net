using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Electrolyte.Extensions;

namespace Electrolyte {
	public class PrivateKeyCollection : IList<PrivateKeyDetails> {
		List<Address> _addresses;
		List<PrivateKeyDetails> _privateKeys;

		public ICollection<Address> Addresses {
			get { return _addresses.AsReadOnly(); }
		}

		public ICollection<PrivateKeyDetails> PrivateKeyDetails {
			get { return _privateKeys.AsReadOnly(); }
		}

		public ICollection<ECKey> PrivateKeys {
			get { return PrivateKeyDetails.Select(k => k.PrivateKey).ToList(); }
		}

		public AddressCollection AddressDetails {
			get { return new AddressCollection(PrivateKeyDetails.Select(k => k.AddressDetails)); }
		}

		public int Count {
			get { return _privateKeys.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}



		public PrivateKeyCollection(IList<PrivateKeyDetails> keys) {
			_addresses = keys.Select(k => k.Address).ToList();
			_privateKeys = keys.ToList();
		}

		public PrivateKeyCollection(IList<ECKey> keys) {
			_addresses = keys.Select(k => k.ToAddress()).ToList();
			_privateKeys = keys.Select(k => new PrivateKeyDetails(k)).ToList();
		}

		public PrivateKeyCollection() {
			_addresses = new List<Address>();
			_privateKeys = new List<PrivateKeyDetails>();
		}

		public PrivateKeyDetails this[int i] {
			get { return _privateKeys[i]; }
			set { _privateKeys[i] = value; }
		}

		public PrivateKeyDetails this[Address address] {
			get { return _privateKeys[_addresses.IndexOf(address)]; }
			set {
				// TODO: Is this setter really necessary?
				int i = _addresses.IndexOf(address);
				if(_privateKeys[i] != value)
					throw new ArgumentException("Provided key does not match address");
			}
		}



		public int IndexOf(PrivateKeyDetails key) {
			return _privateKeys.IndexOf(key);
		}

		public void Insert(int i, ECKey key) {
			Insert(i, new PrivateKeyDetails(key));
		}

		public void Insert(int i, PrivateKeyDetails key) {
			_addresses.Insert(i, key.Address);
			_privateKeys.Insert(i, key);
		}

		public void RemoveAt(int i) {
			_addresses.RemoveAt(i);
			_privateKeys.RemoveAt(i);
		}

		public void Add(string key, string label = null) {
			Add(new PrivateKeyDetails(key, label));
		}

		public void Add(ECKey key, string label = null) {
			Add(new PrivateKeyDetails(key, label));
		}

		public void Add(PrivateKeyDetails details) {
			_addresses.Add(details.Address);
			_privateKeys.Add(details);
		}

		public bool Remove(KeyValuePair<Address, PrivateKeyDetails> pair) {
			try {
				int i = _addresses.IndexOf(pair.Key);
				if(_addresses[i] == pair.Key && _privateKeys[i] == pair.Value) {
					RemoveAt(i);
					return true;
				}
				return false;
			}
			catch {
				return false;
			}
		}

		public bool Remove(PrivateKeyDetails key) {
			try {
				RemoveAt(_privateKeys.IndexOf(key));
				return true;
			}
			catch {
				return false;
			}
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

		public bool Contains(Address address) {
			return _addresses.Contains(address);
		}

		public bool Contains(PrivateKeyDetails key) {
			return _privateKeys.Contains(key);
		}

		public bool TryGetValue(Address address, out PrivateKeyDetails key) {
			try {
				key = this[address];
				return true;
			}
			catch {
				key = null;
				return false;
			}
		}

		public void CopyTo(PrivateKeyDetails[] keys, int index) {
			_privateKeys.CopyTo(keys, index);
		}

		public void Clear() {
			_addresses.Clear();
			_privateKeys.Clear();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _privateKeys.GetEnumerator();
		}

		IEnumerator<PrivateKeyDetails> IEnumerable<PrivateKeyDetails>.GetEnumerator() {
			return _privateKeys.GetEnumerator();
		}

		public Dictionary<Address, PrivateKeyDetails> ToDictionary() {
			return _privateKeys.ToDictionary(k => k.Address, k => k);
		}
	}
}

