using System;
using System.Collections.Generic;
using Electrolyte.Messages;
using Electrolyte.Networking;

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

			Console.Write("Enter an address: ");
			Address address = new Address(Console.ReadLine());

			List<Transaction> txHistory = Network.GetAddressHistory(address);
			Console.WriteLine("Address {0} has {1} transactions", address, txHistory.Count);
		}
	}
}
