using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public static class Network {
		public static NetworkProtocol Protocol;

		static Network() {
			Protocol = new ElectrumProtocol("electrum.be", 50001);
			Connect();
		}

		public static void Connect() {
			Protocol.Connect();
		}

		public static void Disconnect() {
			Protocol.Disconnect();
		}

		public static async Task<Transaction> GetTransactionAsync(TransactionInfo info) {
			return await Protocol.GetTransactionAsync(info);
		}

		public static async Task<Transaction> GetTransactionAsync(string hex, ulong height) {
			return await GetTransactionAsync(new TransactionInfo(hex, height));
		}

		public static async Task<List<Transaction>> GetAddressHistoryAsync(Address address) {
			return await Protocol.GetAddressHistoryAsync(address);
		}

		public static async Task<List<Transaction>> GetAddressHistoryAsync(string address) {
			return await GetAddressHistoryAsync(new Address(address));
		}

		public static async Task<List<Transaction.Output>> GetUnspentOutputsAsync(Address address) {
			return await Protocol.GetUnspentOutputsAsync(address);
		}

		public static async Task<List<Transaction.Output>> GetUnspentOutputsAsync(string address) {
			return await GetUnspentOutputsAsync(new Address(address));
		}

		public static async Task<long> GetAddressBalanceAsync(Address address) {
			return await Protocol.GetAddressBalanceAsync(address);
		}

		public static async Task<long> GetAddressBalanceAsync(string address) {
			return await GetAddressBalanceAsync(new Address(address));
		}
	}
}

