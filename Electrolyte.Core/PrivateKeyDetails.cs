using System;

namespace Electrolyte {
	public class PrivateKeyDetails : AddressDetails {
		public ECKey PrivateKey { get; set; }

		public override Address Address {
			get { return PrivateKey.ToAddress(); }
			set {
				if(PrivateKey.ToAddress() != value)
					throw new ArgumentException("Provided address does not match private key");
			}
		}

		public PrivateKeyDetails(string key, string label = null) : this(ECKey.FromWalletImportFormat(key), label) { }
		public PrivateKeyDetails(byte[] key, string label = null) : this(new ECKey(key), label) { }
		public PrivateKeyDetails(ECKey key, string label = null) {
			PrivateKey = key;
			Label = label;
		}

		public override string ToString() {
			return ToString(false);
		}

		public string ToString(bool wif) {
			if(wif)
				return String.Format(String.IsNullOrEmpty(Label) ? "{0}" : "{0} - {1}", PrivateKey.ToWalletImportFormat(), Label);
			return base.ToString();
		}
	}
}

