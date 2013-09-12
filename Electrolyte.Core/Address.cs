using System;
using Electrolyte.Helpers;

namespace Electrolyte {
	public class Address {
		[Flags]
		public enum Network {
			Bitcoin,
			Friendly,
			Litecoin,
			Namecoin,
			Fairbrix,
			GeistGeld,
			I0coin,
			Testnet,
			Solidcoin,
			Tenebrix,
			Ixcoin,
			Other
		}

		public enum AddressType {
			Public,
			PrivateUncrompressed,
			PrivateCompressed,
			PrivateEncrypted
		}

		protected string _id;
		public string ID {
			get { return _id; }
			set { if(AddressIsValid(value)) _id = value; }
		}

		public Address(string id) {
			ID = id;
		}

		public override string ToString() {
			return ID;
		}

		public override int GetHashCode() {
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj) {
			var address = obj as Address;
			return address != null && ID != null && address.ID != null && ID == address.ID;
		}

		public static bool operator ==(Address a, Address b) {
			return (object)a != null && (object)b != null && a.Equals(b);
		}

		public static bool operator !=(Address a, Address b) {
			return (object)a == null || (object)b == null || !(a == b);
		}

		public static bool AddressIsValid(string address) {
//			byte[] bytes = Base58.DecodeWithChecksum(address);
			Base58.DecodeWithChecksum(address);
			if(!SupportedAddressPrefix(address[0]))
				throw new FormatException(String.Format("'{0}' is an unsupported address prefix", address[0]));

			return true;
		}

		public static bool SupportedAddressPrefix(char prefix) {
			return NetworkFromPrefix(prefix) == Network.Bitcoin;
		}

		public static char PrefixForNetwork(Network network) {
			switch(network) {
			case Network.Bitcoin:
				return '1';
			case Network.Testnet:
				return '2';
			case Network.Friendly:
				return 'F';
			case Network.Litecoin:
				return 'L';
			case Network.Namecoin:
				return 'N';
			case Network.Fairbrix:
				return 'f';
			case Network.GeistGeld:
				return 'g';
			case Network.I0coin:
				return 'j';
			case Network.Solidcoin:
				return 's';
			case Network.Tenebrix:
				return 't';
			default:
				throw new ArgumentException(String.Format("No known network prefix for network {0}", network));
			}
		}

		public static byte BytePrefixForNetwork(Network network) {
			return Base58.Decode(PrefixForNetwork(network).ToString())[0];
		}

		public static Network NetworkFromPrefix(char prefix) {
			switch(prefix) {
			case '1':
			case '3':
				return Network.Bitcoin;
			case '2':
			case '9':
				return Network.Testnet;
			case 'F':
				return Network.Friendly;
			case 'L':
				return Network.Litecoin;
			case 'M':
			case 'N':
				return Network.Namecoin;
			case 'f':
				return Network.Fairbrix;
			case 'g':
				return Network.GeistGeld;
			case 'j':
				return Network.I0coin;
			case 's':
				return Network.Solidcoin;
			case 't':
				return Network.Tenebrix;
			default:
				return Network.Other;
			}
		}

		public static Network NetworkFromPrefix(byte prefix) {
			return NetworkFromPrefix(Base58.Encode(new byte[] { prefix })[0]);
		}
	}
}

