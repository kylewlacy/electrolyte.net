using System;

namespace Electrolyte {
	public class PrivateKeyDetails : AddressDetails {
		protected ECKey _privateKey;
		public ECKey PrivateKey {
			get {
				return _privateKey;
			}
			set {
				_privateKey = value;
				_address = _privateKey.ToAddress();
			}
		}

		protected Address _address;
		public override Address Address {
			get { return _address; }
			set {
				if(_address != value)
					throw new ArgumentException("Address does not match private key");
			}
		}

		public AddressDetails AddressDetails {
			get { return new AddressDetails(this); }
		}

		public PrivateKeyDetails(string key, string label = null) : this(ECKey.FromWalletImportFormat(key), label) { }
		public PrivateKeyDetails(byte[] key, string label = null) : this(new ECKey(key), label) { }
		public PrivateKeyDetails(ECKey key, string label = null) {
			PrivateKey = key;
			Label = label;
		}

		public override string ToString() {
			return String.Format(String.IsNullOrEmpty(Label) ? "{0}" : "{0} - {1}", PrivateKey.ToWalletImportFormat(), Label);
		}
	}
}

