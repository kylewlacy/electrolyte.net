using System;
using System.Collections.Generic;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public static class Network {
		public static NetworkProtocol Protocol;

		static Network() {
			Protocol = new ElectrumProtocol();
			Connect();
		}

		public static void Connect() {
			Protocol.Connect();
		}

		public static void Disconnect() {
			Protocol.Disconnect();
		}

		public static Transaction GetTransaction(TransactionInfo info) {
			return Protocol.GetTransaction(info);
		}

		public static Transaction GetTransaction(string hex, ulong height) {
			return GetTransaction(new TransactionInfo(hex, height));
		}

		public static List<Transaction> GetAddressHistory(Address address) {
			return Protocol.GetAddressHistory(address);
		}

		public static List<Transaction> GetAddressHistory(string address) {
			return GetAddressHistory(new Address(address));
		}
	}
}
