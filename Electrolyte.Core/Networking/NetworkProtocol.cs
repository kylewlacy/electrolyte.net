using System;
using System.Collections.Generic;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public struct TransactionInfo {
		public string Hex;
		public ulong Height;

		public TransactionInfo(string hex, ulong height) {
			Hex = hex;
			Height = height;
		}
	}

	public abstract class NetworkProtocol {
		public abstract void Connect();
		public abstract void Disconnect();
		
		public abstract Transaction GetTransaction(TransactionInfo info);
		public virtual Transaction GetTransaction(string hex, ulong height) {
			return GetTransaction(new TransactionInfo(hex, height));
		}

		public abstract List<Transaction> GetAddressHistory(Address address);
		public virtual List<Transaction> GetAddressHistory(string address) {
			return GetAddressHistory(new Address(address));
		}
	}
}
