using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public static class Network {
		public static List<NetworkProtocol> Protocols = new List<NetworkProtocol>();

		public static NetworkProtocol Protocol {
			get { return Protocols[0]; }
		}

		static Network() {
			Protocols.Add(new ElectrumProtocol("electrum.be", 50001));
			Protocols.Add(new BlockchainProtocol("http://blockchain.info"));
			Protocol.Connect();
		}

		public static void Disconnect() {
			foreach(NetworkProtocol protocol in Protocols)
				protocol.Disconnect();
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

		public static async Task<Money> GetAddressBalanceAsync(Address address) {
			return await Protocol.GetAddressBalanceAsync(address);
		}

		public static async Task<Money> GetAddressBalanceAsync(string address) {
			return await GetAddressBalanceAsync(new Address(address));
		}



		public static async Task<decimal> GetCurrencyConversionRateAsync(Money.CurrencyType c1, Money.CurrencyType c2) {
			return await Protocol.GetCurrencyConversionRateAsync(c1, c2);
		}

		public static async Task<decimal> GetCurrencyConversionRateAsync(string c1, string c2) {
			return await GetCurrencyConversionRateAsync(Money.CurrencyType.FindByCode(c1), Money.CurrencyType.FindByCode(c2));
		}
	}
}

