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

		public async virtual Task<long> GetAddressBalanceAsync(Address address) {
			List<Transaction> addressHistory = await GetAddressHistoryAsync(address);
			Dictionary<Tuple<string, uint>, long> unspentCoins = new Dictionary<Tuple<string, uint>, long>();

			foreach(Transaction tx in addressHistory) {
				for(uint i = 0; i < tx.Outputs.Count; i++) {
					if(tx.Outputs[(int)i].Recipient == address) {
						unspentCoins.Add(Tuple.Create(tx.Hash, i), tx.Outputs[(int)i].Value);
						Console.WriteLine("+{0}:{1} ({2})", tx.Hash, i, tx.Outputs[(int)i].Value);
					}
				}
			}

			foreach(Transaction tx in addressHistory) {
				foreach(Transaction.Input input in tx.Inputs) {
					if(input.Sender == address) {
						if(unspentCoins.ContainsKey(Tuple.Create(input.PrevTransactionHash, input.OutpointIndex))) {
							Console.WriteLine("-{0}:{1} ({2})", input.PrevTransactionHash, input.OutpointIndex, unspentCoins[Tuple.Create(input.PrevTransactionHash, input.OutpointIndex)]);
							unspentCoins.Remove(Tuple.Create(input.PrevTransactionHash, input.OutpointIndex));
						}
					}
				}
			}

			return unspentCoins.Values.Sum();
		}

		public async virtual Task<long> GetAddressBalanceAsync(string address) {
			return await GetAddressBalanceAsync(new Address(address));
		}
	}
}
