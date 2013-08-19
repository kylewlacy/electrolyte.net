using System;
using System.IO;
using System.Collections.Generic;
using Electrolyte.Messages;
using Electrolyte.Networking;
using Electrolyte.Primitives;

namespace Electrolyte.CLI {
	class MainClass {
		public static void Main(string[] args) {
//			Console.Write("Enter a transaction: ");
//			string hex = Console.ReadLine();
//
//			Console.Write("Enter tx height: ");
//			ulong height = UInt64.Parse(Console.ReadLine());
//
//			Transaction tx = protocol.GetTransaction(hex, height);
//
//			Console.WriteLine("First output script pubkey is:\n{0}", tx.Outputs[0].ScriptPubKey);



			// [<network prefix>] + ScriptPubKey["pubKeyHash"].HexToByteArray() == Sender Address bytes
			// [<network prefix>] + ripemd(sha25(ScriptSig["pubKey"])) == Recipient Address bytes


//			Console.Write("Enter an address: ");
//			Address address = new Address(Console.ReadLine());
//
//			List<Transaction> txHistory = Network.GetAddressHistory(address);
//			Console.WriteLine("Address {0} has {1} transactions", address, txHistory.Count);
//			Console.WriteLine("==================");
//
//			foreach(Transaction tx in txHistory) {
//				Console.WriteLine("{0}", tx.Hash);
//				Console.WriteLine("  Senders");
//				foreach(Address sender in tx.Senders) {
//					Console.WriteLine("    {0}", sender);
//				}
//
//				Console.WriteLine("  Recipients");
//				foreach(Address recipient in tx.Recipients) {
//					Console.WriteLine("    {0}", recipient);
//				}
//			}

//			Console.Write("Enter an address: ");
//			Address address = new Address(Console.ReadLine());
//
//			Console.WriteLine("Balance: {0}", Network.GetAddressBalanceAsync(address).Result);

			Console.Write("Enter a private key: ");
			ECKey key = ECKey.FromWalletImportFormat(Console.ReadLine());

			Console.WriteLine("Getting unspent transactions for {0}...", key.ToAddress());
			List<Transaction.Output> unspentOutputs = Network.GetUnspentOutputsAsync(key.ToAddress()).Result;
			Console.WriteLine("Done");

			for(int i = 0; i < unspentOutputs.Count; i++) {
				Console.WriteLine("{0} - {1}:{2} ({3})", i, unspentOutputs[i].Transaction.Hash, unspentOutputs[i].Index, unspentOutputs[i].Value);
			}
			Console.Write("Pick an output: ");
			Transaction.Output inpoint = unspentOutputs[Int32.Parse(Console.ReadLine())];

			Console.Write("Enter a recipient: ");
			Address recipient = new Address(Console.ReadLine());

			Dictionary<Address, long> destinations = new Dictionary<Address, long> { { recipient, inpoint.Value - 100000L} };
			Dictionary<string, ECKey> privateKeys = new Dictionary<string, ECKey> { { key.ToAddress().ID, key } };

			Transaction tx = Transaction.Create(new List<Transaction.Output> { inpoint }, destinations, privateKeys);

			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					tx.WritePayload(writer);
					Console.WriteLine(BitConverter.ToString(stream.ToArray()).Replace("-", "").ToLower());
				}
			}
		}
	}
}
