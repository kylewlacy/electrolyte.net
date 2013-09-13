using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electrolyte.Extensions;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public static class Network {
		public static List<NetworkProtocol> Protocols = new List<NetworkProtocol>();

		public static List<CacheProtocol> CacheProtocols {
			get {
				return Protocols.OfType<CacheProtocol>().ToList();
			}
		}

		public static NetworkProtocol Protocol {
			get { return Protocols[0]; }
		}

		public static CacheProtocol CacheProtocol {
			get { return CacheProtocols[0]; }
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

		public static async Task<List<Task<Transaction>>> GetAddressHistoryListAsync(Address address, ulong startHeight = 0) {
			return await Protocol.GetAddressHistoryListAsync(address, startHeight);
		}

		public static async Task<List<Task<Transaction>>> GetAddressHistoryListAsync(string address, ulong startHeight = 0) {
			return await GetAddressHistoryListAsync(new Address(address), startHeight);
		}

		public static async Task<List<Transaction>> GetAddressHistoryAsync(Address address, ulong startHeight = 0) {
			return await Protocol.GetAddressHistoryAsync(address, startHeight);
		}

		public static async Task<List<Transaction>> GetAddressHistoryAsync(string address, ulong startHeight = 0) {
			return await GetAddressHistoryAsync(new Address(address), startHeight);
		}

		public static async Task<List<Transaction.Output>> GetUnspentOutputsAsync(Address address, ulong startHeight = 0) {
			return await Protocol.GetUnspentOutputsAsync(address, startHeight);
		}

		public static async Task<List<Transaction.Output>> GetUnspentOutputsAsync(string address, ulong startHeight = 0) {
			return await GetUnspentOutputsAsync(new Address(address), startHeight);
		}

		public static async Task<List<Transaction.Output>> GetUnspentOutputsAsync(List<Address> addresses, ulong startHeight = 0) {
			if(addresses.Count <= 0) return new List<Transaction.Output>();

			var outputs = new List<Transaction.Output>();
			foreach(var address in addresses)
				outputs.AddRange(await Network.GetUnspentOutputsAsync(address, startHeight));

			return outputs;
		}



		public static async Task<List<Transaction.Delta>> GetDeltasForAddressesAsync(ICollection<Address> addresses, ulong startHeight = 0) {
			var deltas = new Dictionary<Transaction, Money>();

			// TODO: Order this by block height to avoid two `foreach` loops
			// Two loops exist because 'subtract' inputs may come before 'add' outputs
			List<List<Transaction>> historyList = (await Task.WhenAll(addresses.Select(async a => await Network.GetAddressHistoryAsync(a, startHeight)).ToArray())).ToList();
			List<Transaction> history = historyList.SelectMany(h => h).ToList();

			foreach(var tx in history) {
				foreach(var output in tx.Outputs) {
					if(addresses.Contains(output.Recipient)) {
						if(!deltas.ContainsKey(tx)) { deltas.Add(tx, Money.Zero("BTC")); }
						deltas[tx] += output.Value;
					}
				}
			}

			foreach(var tx in history) {
				foreach(var input in tx.Inputs) {
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

		public static async Task<Money> GetAddressBalanceAsync(Address address, ulong startHeight = 0) {
			return await Protocol.GetAddressBalanceAsync(address, startHeight);
		}

		public static async Task<Money> GetAddressBalanceAsync(string address, ulong startHeight = 0) {
			return await GetAddressBalanceAsync(new Address(address), startHeight);
		}

		public static async Task<Money> GetAddressBalancesAsync(ICollection<Address> addresses, ulong startHeight = 0) {
			if(addresses.Count <= 0) return Money.Zero("BTC");
			IEnumerable<Task<Money>> balances = addresses.Select(async a => await Network.GetAddressBalanceAsync(a, startHeight));
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



		public static async Task<Money> GetCachedBalanceAsync(Address address, ulong startHeight = 0) {
			return await CacheProtocol.GetCachedBalanceAsync(address, startHeight);
		}

		public static async Task<Money> GetCachedBalanceAsync(string address, ulong startHeight = 0) {
			return await GetCachedBalanceAsync(new Address(address), startHeight);
		}

		public static async Task<Money> GetCachedBalancesAsync(ICollection<Address> addresses, ulong startHeight = 0) {
			if(addresses.Count <= 0) return Money.Zero("BTC");
			IEnumerable<Task<Money>> balances = addresses.Select(async a => await Network.GetCachedBalanceAsync(a, startHeight));
			return (await Task.WhenAll(balances)).Sum();
		}
	}
}

