using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Electrolyte.Portable;
using Electrolyte.Portable.Networking;
using Electrolyte.Messages;
using Electrolyte.Helpers;
using Electrolyte.Extensions;

namespace Electrolyte.Networking {
	public class ElectrumProtocol : NetworkProtocol {
		public string Server;
		public int Port;

		protected TcpStream Client;
		protected virtual Stream ClientStream {
			get { return Client; }
		}

		protected static readonly SemaphoreLite ClientLock = new SemaphoreLite();

		public ElectrumProtocol(string server, int port) {
			Server = server;
			Port = port;
		}
		

		public override void Connect() {
			base.Connect();
			Client = TcpStream.Create(Server, Port);
			Client.Connect();
		}

		public override void Disconnect() {
			base.Disconnect();
			ClientStream.Dispose();
		}

		async Task<string> SendRPCAsync(string methodName, params object[] args) {
			var jsonArgs = new List<string>();
			foreach(var arg in args) {
				if(arg is string) {
					jsonArgs.Add(String.Format("\"{0}\"", arg));
				}
				else {
					jsonArgs.Add(arg.ToString());
				}
			}

			var rpc = String.Format("{{\"id\": 1, \"method\": \"{0}\", \"params\": [{1}]}}", methodName, String.Join(", ", jsonArgs));

			await ClientLock.WaitAsync();
			try {
				var writer = new StreamWriter(ClientStream);
				await writer.WriteLineAsync(rpc);
				await writer.FlushAsync();

				var reader = new StreamReader(ClientStream);
				return await reader.ReadLineAsync();
			}
			finally {
				ClientLock.Release();
			}
		}

		public async override Task<Transaction> GetTransactionAsync(Transaction.Info info) {
			var json = JToken.Parse(await SendRPCAsync("blockchain.transaction.get", info.Hash, info.Height));

			var rawTx = BinaryHelpers.HexToByteArray(json["result"].Value<string>());
			var tx = new Transaction(info.Height);
			tx.ReadPayload(new BinaryReader(new MemoryStream(rawTx)));

			return tx;
		}

		public async override Task<List<Task<Transaction>>> GetAddressHistoryListAsync(Address address, ulong startHeight = 0) {
			var json = JToken.Parse(await SendRPCAsync("blockchain.address.get_history", address.ID));
			List<Transaction.Info> transactions = json["result"].Select(t => new Transaction.Info(t["tx_hash"].Value<string>(), t["height"].Value<ulong>())).ToList();

			return transactions.Where(i => i.Height >= startHeight).Select(Network.GetTransactionAsync).ToList();
		}

		public async override Task<Money> GetAddressBalanceAsync(Address address, ulong startHeight = 0) {
			List<Transaction.Output> unspentOutputs = await Network.GetUnspentOutputsAsync(address, startHeight);
			return unspentOutputs.Select(o => o.Value).Sum();
		}



		public override async Task BroadcastTransactionAsync(Transaction tx) {
			var json = JToken.Parse(await SendRPCAsync("blockchain.transaction.broadcast", tx.ToHex()));
			var result = json["result"].Value<string>();

			int? errorCode = null;
			try {
				// TODO: Handle Python string literals properly
				var resultError = JToken.Parse(Regex.Replace(result, "u'(?<text>[^']*)'", "\"${text}\""));

				errorCode = resultError["code"].Value<int>();
				Debug.WriteLine(String.Format("There was an error: {0} ({1})\n{2}", errorCode, resultError["message"], result));
			}
			catch { }

			switch(errorCode) {
			case -22:
				Debug.WriteLine("Transaction not accepted by Electrum server");
				await NextProtocol.BroadcastTransactionAsync(tx);
				break;
			default:
				if(json["result"].Value<string>() != tx.Hash)
					throw new Exception("Wat");
				break;
			}

			// TODO: Cache the transaction
		}
	}
}

