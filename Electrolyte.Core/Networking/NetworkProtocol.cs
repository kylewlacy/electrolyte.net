using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electrolyte.Extensions;
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
		public class NotConnectedException : System.InvalidOperationException {
			public NotConnectedException() { }
			public NotConnectedException(string message) : base(message) { }
			public NotConnectedException(string message, Exception inner) : base(message, inner) { }
		}

		public bool IsConnected { get; private set; }

		public NetworkProtocol NextProtocol {
			get {
				int nextIndex = Network.Protocols.LastIndexOf(this) + 1;
				if(nextIndex >= Network.Protocols.Count)
					throw new InvalidOperationException();
				NetworkProtocol next = Network.Protocols[nextIndex];
				if(!next.IsConnected)
					next.Connect();
				return next;
			}
		}

		public virtual void Connect() {
			IsConnected = true;
		}

		public virtual void Disconnect() {
			IsConnected = false;
		}
		
		public async virtual Task<Transaction> GetTransactionAsync(TransactionInfo info) {
			return await NextProtocol.GetTransactionAsync(info);
		}

		public async virtual Task<Transaction> GetTransactionAsync(string hex, ulong height) {
			return await GetTransactionAsync(new TransactionInfo(hex, height));
		}

		public async virtual Task<List<Transaction>> GetAddressHistoryAsync(Address address) {
			return await NextProtocol.GetAddressHistoryAsync(address);
		}

		public async virtual Task<List<Transaction>> GetAddressHistoryAsync(string address) {
			return await GetAddressHistoryAsync(new Address(address));
		}

		public async Task<List<Transaction.Output>> GetUnspentOutputsAsync(Address address) {
			List<Transaction> addressHistory = await GetAddressHistoryAsync(address);
			Dictionary<Tuple<string, uint>, Transaction.Output> unspentOutputs = new Dictionary<Tuple<string, uint>, Transaction.Output>();

			foreach(Transaction tx in addressHistory) {
				foreach(Transaction.Output output in tx.Outputs) {
					if(output.Recipient == address) {
						unspentOutputs.Add(Tuple.Create(output.Transaction.Hash, output.Index), output);
//						Console.WriteLine("+{0}:{1} ({2})", tx.Hash, output.Index, output.Value);
					}
				}
			}

			foreach(Transaction tx in addressHistory) {
				foreach(Transaction.Input input in tx.Inputs) {
					if(input.Sender == address) {
						// TODO: Provide some means of an input returning an equivalent (so we can use a `List` rather than a `Dictionary`
						if(unspentOutputs.ContainsKey(Tuple.Create(input.PrevTransactionHash, input.OutpointIndex))) {
//							Console.WriteLine("-{0}:{1} ({2})", input.PrevTransactionHash, input.OutpointIndex, unspentOutputs[Tuple.Create(input.PrevTransactionHash, input.OutpointIndex)].Value);
							unspentOutputs.Remove(Tuple.Create(input.PrevTransactionHash, input.OutpointIndex));
						}
					}
				}
			}

			return unspentOutputs.Values.ToList();
		}

		public async Task<List<Transaction.Output>> GetUnspentOutputsAsync(string address) {
			return await GetUnspentOutputsAsync(new Address(address));
		}

		public async virtual Task<Money> GetAddressBalanceAsync(Address address) {
			List<Transaction.Output> unspentOutputs = await GetUnspentOutputsAsync(address);
			return unspentOutputs.Select(o => o.Value).Sum();
		}

		public async virtual Task<Money> GetAddressBalanceAsync(string address) {
			return await GetAddressBalanceAsync(new Address(address));
		}



		public async virtual Task BroadcastTransactionAsync(Transaction tx) {
			await NextProtocol.BroadcastTransactionAsync(tx);
		}

		

		public virtual async Task<decimal> GetExchangeRateAsync(Money.CurrencyType c1, Money.CurrencyType c2) {
			return await NextProtocol.GetExchangeRateAsync(c1, c2);
		}

		public virtual async Task<decimal> GetExchangeRateAsync(string c1, string c2) {
			return await GetExchangeRateAsync(Money.CurrencyType.FindByCode(c1), Money.CurrencyType.FindByCode(c2));
		}
	}
}
