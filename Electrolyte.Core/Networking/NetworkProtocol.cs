using System;
using System.Linq;
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

		public virtual long GetAddressBalance(Address address) {
			List<Transaction> addressHistory = GetAddressHistory(address);
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
						string prevTxHash = BitConverter.ToString(input.PrevTransactionHash.Reverse().ToArray()).Replace("-", "").ToLower();
						if(unspentCoins.ContainsKey(Tuple.Create(prevTxHash, input.OutpointIndex))) {
							Console.WriteLine("-{0}:{1} ({2})", prevTxHash, input.OutpointIndex, unspentCoins[Tuple.Create(prevTxHash, input.OutpointIndex)]);
							unspentCoins.Remove(Tuple.Create(prevTxHash, input.OutpointIndex));
						}
					}
				}
			}

			return unspentCoins.Values.Sum();
		}

		public virtual long GetAddressBalance(string address) {
			return GetAddressBalance(new Address(address));
		}
	}
}
