using System;
using System.IO;
using System.Text;
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

		TcpClient Client;
		static readonly SemaphoreSlim clientLock = new SemaphoreSlim(1);

		public ElectrumProtocol(string server, int port) {
			Client = new TcpClient();
			Server = server;
			Port = port;
		}
		

		public override void Connect() {
			base.Connect();
			Client.Connect(Server, Port);
		}

		public override void Disconnect() {
			base.Disconnect();
			Client.Close();
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
				StreamWriter writer = new StreamWriter(Client.GetStream());
				await writer.WriteLineAsync(rpc);
				await writer.FlushAsync();

				StreamReader reader = new StreamReader(Client.GetStream());
				return await reader.ReadLineAsync();
			}
			finally {
				clientLock.Release();
			}
		}

		public async override Task<Transaction> GetTransactionAsync(TransactionInfo info) {
			JToken json = JToken.Parse(await SendRPCAsync("blockchain.transaction.get", info.Hex, info.Height));

			string txHex = json["result"].Value<string>();
			byte[] rawTx = BinaryHelpers.HexToByteArray(txHex);

			Transaction tx = new Transaction();
			tx.ReadPayload(new BinaryReader(new MemoryStream(rawTx)));
			return tx;
		}

		public async override Task<List<Task<Transaction>>> GetAddressHistoryListAsync(Address address) {
			List<Task<Transaction>> transactionTasks = new List<Task<Transaction>>();
			JToken json = JToken.Parse(await SendRPCAsync("blockchain.address.get_history", address.ID));

			foreach(JToken tx in json["result"])
				transactionTasks.Add(Network.GetTransactionAsync(tx["tx_hash"].Value<string>(), tx["height"].Value<ulong>()));

			return transactionTasks;
		}

		public async override Task<Money> GetAddressBalanceAsync(Address address) {
			List<Transaction.Output> unspentOutputs = await Network.GetUnspentOutputsAsync(address);
			return unspentOutputs.Select(o => o.Value).Sum();
		}



		public override async Task BroadcastTransactionAsync(Transaction tx) {
			JToken json = JToken.Parse(await SendRPCAsync("blockchain.transaction.broadcast", tx.ToHex()));

			if(json["result"].Value<string>() != tx.Hash)
				throw new Exception("Wat");

			// TODO: Cache the transaction
		}
	}
}

