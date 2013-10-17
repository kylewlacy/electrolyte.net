using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Electrolyte.Extensions;

namespace Electrolyte {
	public class PrivateKeyCollection : IList<PrivateKeyDetails>, IDictionary<Address, PrivateKeyDetails> {
		List<Address> Addresses;
		List<PrivateKeyDetails> PrivateKeys;

		public ICollection<Address> Keys {
			get { return Addresses.AsReadOnly(); }
		}

		public ICollection<PrivateKeyDetails> Values {
			get { return PrivateKeys.AsReadOnly(); }
		}

		Dictionary<Address, PrivateKeyDetails> Dictionary {
			get { return PrivateKeys.ToDictionary(k => k.Address, k => k); }
		}

		public int Count {
			get { return PrivateKeys.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}



		public PrivateKeyCollection() {
			Addresses = new List<Address>();
			PrivateKeys = new List<PrivateKeyDetails>();
		}

		public PrivateKeyDetails this[int i] {
			get { return PrivateKeys[i]; }
			set { PrivateKeys[i] = value; }
		}

		public PrivateKeyDetails this[Address address] {
			get { return PrivateKeys[Addresses.IndexOf(address)]; }
			set {
				// TODO: Is this setter really necessary?
				int i = Addresses.IndexOf(address);
				if(PrivateKeys[i] != value)
					throw new ArgumentException("Provided key does not match address");
			}
		}



		public int IndexOf(PrivateKeyDetails key) {
			return PrivateKeys.IndexOf(key);
		}

		public void Insert(int i, ECKey key) {
			Insert(i, new PrivateKeyDetails(key));
		}

		public void Insert(int i, PrivateKeyDetails key) {
			Addresses.Insert(i, key.Address);
			PrivateKeys.Insert(i, key);
		}

		public void RemoveAt(int i) {
			Addresses.RemoveAt(i);
			PrivateKeys.RemoveAt(i);
		}

		public void Add(KeyValuePair<Address, PrivateKeyDetails> pair) {
			Add(pair.Key, pair.Value);
		}

		public void Add(Address address, PrivateKeyDetails key) {
			if(key.Address != address)
				throw new ArgumentException("Provided key did not match address", "key");
			Add(key);
		}

		public void Add(ECKey key) {
			Add(new PrivateKeyDetails(key));
		}

		public void Add(PrivateKeyDetails details) {
			Addresses.Add(details.Address);
			PrivateKeys.Add(details);
		}

		public bool Remove(KeyValuePair<Address, PrivateKeyDetails> pair) {
			try {
				int i = Addresses.IndexOf(pair.Key);
				if(Addresses[i] == pair.Key && PrivateKeys[i] == pair.Value) {
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
				RemoveAt(PrivateKeys.IndexOf(key));
				return true;
			}
			catch {
				return false;
			}
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

		public bool ContainsKey(Address address) {
			return Addresses.Contains(address);
		}

		public bool Contains(PrivateKeyDetails key) {
			return PrivateKeys.Contains(key);
		}

		public bool Contains(KeyValuePair<Address, PrivateKeyDetails> pair) {
			return ContainsKey(pair.Key) && PrivateKeys[Addresses.IndexOf(pair.Key)] == pair.Value;
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
			PrivateKeys.CopyTo(keys, index);
		}

		public void CopyTo(KeyValuePair<Address, PrivateKeyDetails>[] pairs, int index) {
			Dictionary.Select(p => p).ToList().CopyTo(pairs, index);
		}

		public void Clear() {
			Addresses.Clear();
			PrivateKeys.Clear();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return PrivateKeys.GetEnumerator();
		}

		IEnumerator<PrivateKeyDetails> IEnumerable<PrivateKeyDetails>.GetEnumerator() {
			return PrivateKeys.GetEnumerator();
		}

		IEnumerator<KeyValuePair<Address, PrivateKeyDetails>> IEnumerable<KeyValuePair<Address, PrivateKeyDetails>>.GetEnumerator() {
			return Dictionary.GetEnumerator();
		}
	}
}

