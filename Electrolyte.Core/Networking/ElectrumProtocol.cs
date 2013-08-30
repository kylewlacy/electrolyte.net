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

		async Task SendRPCAsync(string methodName, params object[] args) {
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
			}
			finally {
				clientLock.Release();
			}
		}

		async Task<string> GetResponseAsync() {
			await clientLock.WaitAsync();
			try {
				StreamReader reader = new StreamReader(Client.GetStream());
				return await reader.ReadLineAsync();
			}
			finally {
				clientLock.Release();
			}
		}

		public async override Task<Transaction> GetTransactionAsync(TransactionInfo info) {
			await SendRPCAsync("blockchain.transaction.get", info.Hex, info.Height);
			JToken json = JToken.Parse(await GetResponseAsync());

			string txHex = json["result"].Value<string>();

			byte[] rawTx = BinaryHelpers.HexToByteArray(txHex);

			Transaction tx = new Transaction();
			tx.ReadPayload(new BinaryReader(new MemoryStream(rawTx)));
			return tx;
		}

		public async override Task<List<Transaction>> GetAddressHistoryAsync(Address address) {
			await SendRPCAsync("blockchain.address.get_history", address.ID);

			List<Transaction> transactions = new List<Transaction>();
			JToken json = JToken.Parse(await GetResponseAsync());

			foreach(JToken tx in json["result"])
				transactions.Add(await Network.GetTransactionAsync(tx["tx_hash"].Value<string>(), tx["height"].Value<ulong>()));

			return transactions;
		}

		public async override Task<Money> GetAddressBalanceAsync(Address address) {
			List<Transaction.Output> unspentOutputs = await Network.GetUnspentOutputsAsync(address);
			return unspentOutputs.Select(o => o.Value).Sum();
		}



		public override async Task BroadcastTransactionAsync(Transaction tx) {
			await SendRPCAsync("blockchain.transaction.broadcast", tx.ToHex());
			JToken json = JToken.Parse(await GetResponseAsync());

			if(json["result"].Value<string>() != tx.Hash)
				throw new Exception("Wat");

			// TODO: Cache the transaction
		}
	}
}

