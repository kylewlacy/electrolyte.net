using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Electrolyte.Messages;
using Electrolyte.Helpers;
using Electrolyte.Extensions;

namespace Electrolyte.Networking {
	public class ElectrumProtocol : NetworkProtocol {
		public string Server;
		public int Port;

		protected TcpClient Client;
		protected Stream ClientStream;
		protected static readonly SemaphoreSlim clientLock = new SemaphoreSlim(1);

		public ElectrumProtocol(string server, int port) {
			Client = new TcpClient();
			Server = server;
			Port = port;
		}
		

		public override void Connect() {
			base.Connect();
			Client.Connect(Server, Port);
			ClientStream = Client.GetStream();
		}

		public override void Disconnect() {
			base.Disconnect();
			ClientStream.Close();
			Client.Close();
			ClientStream = null;
		}

		async Task<string> SendRPCAsync(string methodName, params object[] args) {
			List<string> jsonArgs = new List<string>();
			foreach(object arg in args) {
				if(arg is string) {
					jsonArgs.Add(String.Format("\"{0}\"", arg));
				}
				else {
					jsonArgs.Add(arg.ToString());
				}
			}

			string rpc = String.Format("{{\"id\": 1, \"method\": \"{0}\", \"params\": [{1}]}}", methodName, String.Join(", ", jsonArgs));

			await clientLock.WaitAsync();
			try {
				StreamWriter writer = new StreamWriter(ClientStream);
				await writer.WriteLineAsync(rpc);
				await writer.FlushAsync();

				StreamReader reader = new StreamReader(ClientStream);
				return await reader.ReadLineAsync();
			}
			finally {
				clientLock.Release();
			}
		}

		public async override Task<Transaction> GetTransactionAsync(Transaction.Info info) {
			JToken json = JToken.Parse(await SendRPCAsync("blockchain.transaction.get", info.Hash, info.Height));

			string txHex = json["result"].Value<string>();
			byte[] rawTx = BinaryHelpers.HexToByteArray(txHex);

			Transaction tx = new Transaction(info.Height);
			tx.ReadPayload(new BinaryReader(new MemoryStream(rawTx)));
			return tx;
		}

		public async override Task<List<Task<Transaction>>> GetAddressHistoryListAsync(Address address, ulong startHeight = 0) {
			List<Task<Transaction>> transactionTasks = new List<Task<Transaction>>();
			JToken json = JToken.Parse(await SendRPCAsync("blockchain.address.get_history", address.ID));
			List<Transaction.Info> transactions = json["result"].Select(t => new Transaction.Info(t["tx_hash"].Value<string>(), t["height"].Value<ulong>())).ToList();

			foreach(Transaction.Info tx in transactions.Where(i => i.Height >= startHeight))
				transactionTasks.Add(Network.GetTransactionAsync(tx));

			return transactionTasks;
		}

		public async override Task<Money> GetAddressBalanceAsync(Address address, ulong startHeight = 0) {
			List<Transaction.Output> unspentOutputs = await Network.GetUnspentOutputsAsync(address, startHeight);
			return unspentOutputs.Select(o => o.Value).Sum();
		}



		public override async Task BroadcastTransactionAsync(Transaction tx) {
			JToken json = JToken.Parse(await SendRPCAsync("blockchain.transaction.broadcast", tx.ToHex()));
			string result = json["result"].Value<string>();

			int? errorCode = null;
			try {
				// TODO: Handle Python string literals properly
				JToken resultError = JToken.Parse(Regex.Replace(result, "u'(?<text>[^']*)'", "\"${text}\""));

				errorCode = resultError["code"].Value<int>();
				Console.WriteLine("There was an error: {0} ({1})\n{2}", errorCode, resultError["message"], result);
			}
			catch { }

			switch(errorCode) {
			case -22:
				Console.WriteLine("Transaction not accepted by Electrum server");
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

