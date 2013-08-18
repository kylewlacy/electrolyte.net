using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public class ElectrumProtocol : NetworkProtocol {
		public string Server;
		public int Port;
		public TcpClient Client;

		public ElectrumProtocol(string server, int port) {
			Client = new TcpClient();
			Server = server;
			Port = port;
		}
		

		public override void Connect() {
			Client.Connect(Server, Port);
		}

		public override void Disconnect() {
			Client.Close();
		}

		void SendRPC(string methodName, params object[] args) {
			List<string> jsonArgs = new List<string>();
			foreach(object arg in args) {
				if(arg is string) {
					jsonArgs.Add(String.Format("\"{0}\"", arg));
				}
				else {
					jsonArgs.Add(arg.ToString());
				}
			}

			string rpc = String.Format("{{\"id\": 1, \"method\": \"{0}\", \"params\": [{1}]}}\n", methodName, String.Join(", ", jsonArgs));
			byte[] bytes = Encoding.ASCII.GetBytes(rpc);
			Client.GetStream().Write(bytes, 0, bytes.Length);
		}

		string GetResponse() {
			StreamReader reader = new StreamReader(Client.GetStream());
			return reader.ReadLine();
		}

		public override Transaction GetTransaction(TransactionInfo info) {
			SendRPC("blockchain.transaction.get", info.Hex, info.Height);
			JToken json = JToken.Parse(GetResponse());
			string txHex = json["result"].Value<string>();

			// http://stackoverflow.com/a/13228503/1311454
			// TODO: Rewrite faster
			byte[] rawTx = Enumerable.Range(0, txHex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(txHex.Substring(x, 2), 16)).ToArray();

			Transaction tx = new Transaction();
			tx.ReadPayload(new BinaryReader(new MemoryStream(rawTx)));
			return tx;
		}

		public override List<Transaction> GetAddressHistory(Address address) {
			SendRPC("blockchain.address.get_history", address.ID);

			List<Transaction> transactions = new List<Transaction>();
			JToken json = JToken.Parse(GetResponse());
			foreach(JToken tx in json["result"]) {
				transactions.Add(GetTransaction(tx["tx_hash"].Value<string>(), tx["height"].Value<ulong>()));
			}

			return transactions;
		}
	}
}

