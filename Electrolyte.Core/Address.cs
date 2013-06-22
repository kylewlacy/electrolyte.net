using System;
using System.Collections.Generic;
using System.Linq;
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

		public string ToString() {
			return ID;
		}

		public static bool AddressIsValid(string address) {
			byte[] bytes = Base58.DecodeWithChecksum(address);
			if(!SupportedAddressPrefix(address[0]))
				throw new FormatException(String.Format("'{0}' is an unsupported address prefix", address[0]));

			return true;
		}

		public static bool SupportedAddressPrefix(char prefix) {
			return PrefixNetwork(prefix) == Network.Bitcoin;
		}

		public static Network PrefixNetwork(char prefix) {
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
	}
}

