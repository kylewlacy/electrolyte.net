using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electrolyte.Extensions;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public static class Network {
		public static List<NetworkProtocol> Protocols = new List<NetworkProtocol>();

		public static NetworkProtocol Protocol {
			get { return Protocols[0]; }
		}

		static Network() {
			Protocols.Add(new MemoryCacheProtocol());
			Protocols.Add(new FileCacheProtocol());
			Protocols.Add(new SecureElectrumProtocol("btc.medoix.com", 50002));
			Protocols.Add(new BlockchainProtocol("http://blockchain.info"));
			Protocol.Connect();
		}

		public static void Disconnect() {
			foreach(NetworkProtocol protocol in Protocols)
				protocol.Disconnect();
		}

		public static async Task<Transaction> GetTransactionAsync(Transaction.Info info) {
			return await Protocol.GetTransactionAsync(info);
		}

		public static async Task<Transaction> GetTransactionAsync(string hex, ulong height) {
			return await GetTransactionAsync(new Transaction.Info(hex, height));
		}

		public static async Task<List<Task<Transaction>>> GetAddressHistoryListAsync(Address address) {
			return await Protocol.GetAddressHistoryListAsync(address);
		}

		public static async Task<List<Task<Transaction>>> GetAddressHistoryListAsync(string address) {
			return await GetAddressHistoryListAsync(new Address(address));
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

		public static async Task<List<Transaction.Output>> GetUnspentOutputsAsync(List<Address> addresses) {
			if(addresses.Count <= 0) return new List<Transaction.Output>();

			List<Transaction.Output> outputs = new List<Transaction.Output>();
			foreach(Address address in addresses)
				outputs.AddRange(await Network.GetUnspentOutputsAsync(address));

			return outputs;
		}



		public static async Task<List<Transaction.Delta>> GetDeltasForAddressesAsync(List<Address> addresses) {
			Dictionary<Transaction, Money> deltas = new Dictionary<Transaction, Money>();

			// TODO: Order this by block height to avoid two `foreach` loops
			// Two loops exist because 'subtract' inputs may come before 'add' outputs
			List<List<Transaction>> historyList = (await Task.WhenAll(addresses.Select(async (a) => await Network.GetAddressHistoryAsync(a)).ToArray())).ToList();
			List<Transaction> history = historyList.SelectMany(h => h).ToList();

			foreach(Transaction tx in history) {
				foreach(Transaction.Output output in tx.Outputs) {
					if(addresses.Contains(output.Recipient)) {
						if(!deltas.ContainsKey(tx)) { deltas.Add(tx, Money.Zero("BTC")); }
						deltas[tx] += output.Value;
					}
				}
			}

			foreach(Transaction tx in history) {
				foreach(Transaction.Input input in tx.Inputs) {
					if(addresses.Contains(input.Sender)) {
						Transaction prevTx = deltas.Keys.FirstOrDefault(t => input.PrevTransactionHash == t.Hash);
						if(deltas.ContainsKey(prevTx)) {
							if(!deltas.ContainsKey(tx)) { deltas.Add(tx, Money.Zero("BTC")); }
							deltas[tx] -= prevTx.Outputs[(int)input.OutpointIndex].Value;
						}
					}
				}
			}

			return deltas.Select(p => new Transaction.Delta(p.Key, p.Value)).ToList();
		}

		public static async Task<Money> GetAddressBalanceAsync(Address address) {
			return await Protocol.GetAddressBalanceAsync(address);
		}

		public static async Task<Money> GetAddressBalanceAsync(string address) {
			return await GetAddressBalanceAsync(new Address(address));
		}

		public static async Task<Money> GetAddressBalancesAsync(List<Address> addresses) {
			if(addresses.Count <= 0) return Money.Zero("BTC");
			IEnumerable<Task<Money>> balances = addresses.Select(async (a) => await Network.GetAddressBalanceAsync(a));
			return (await Task.WhenAll(balances)).Sum();
		}



		public static async Task BroadcastTransactionAsync(Transaction tx) {
			await Protocol.BroadcastTransactionAsync(tx);
		}



		public static async Task<decimal> GetExchangeRateAsync(Money.CurrencyType c1, Money.CurrencyType c2) {
			return await Protocol.GetExchangeRateAsync(c1, c2);
		}

		public static async Task<decimal> GetExchangeRateAsync(string c1, string c2) {
			return await GetExchangeRateAsync(Money.CurrencyType.FindByCode(c1), Money.CurrencyType.FindByCode(c2));
		}
	}
}

