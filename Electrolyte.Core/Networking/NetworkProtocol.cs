using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		
		public abstract Task<Transaction> GetTransactionAsync(TransactionInfo info);
		public async virtual Task<Transaction> GetTransactionAsync(string hex, ulong height) {
			return await GetTransactionAsync(new TransactionInfo(hex, height));
		}

		public abstract Task<List<Transaction>> GetAddressHistoryAsync(Address address);
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

		public async virtual Task<long> GetAddressBalanceAsync(Address address) {
			List<Transaction.Output> unspentOutputs = await GetUnspentOutputsAsync(address);
			return unspentOutputs.Select(o => o.Value).Sum();
		}

		public async virtual Task<long> GetAddressBalanceAsync(string address) {
			return await GetAddressBalanceAsync(new Address(address));
		}
	}
}
