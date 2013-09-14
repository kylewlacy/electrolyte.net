using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Electrolyte.Helpers;
using Electrolyte.Messages;
using Electrolyte.Networking;
using Electrolyte.Primitives;

namespace Electrolyte.CLI {
	class MainClass {
		public static Wallet Wallet;

		public static int Main(string[] args) {
			string walletLocation = Wallet.DefaultWalletPath;
			if(File.Exists(walletLocation)) {
				Wallet = Wallet.LoadAsync(walletLocation).Result;
			}
			else {
				Console.Write("Enter a passphrase for your wallet: ");
				Wallet = Wallet.CreateAsync(ReadSecureLine(), walletLocation).Result;
			}

			while(true) {
				Console.Write("{0} > ", Wallet.IsLocked ? "locked  " : "unlocked");
				int? statusCode = ProcessCommand(Console.ReadLine()).Result;

				if(statusCode.HasValue)
					return statusCode.Value;
			}
		}

		public static async Task<int?> ProcessCommand(string commands) {
			return await ProcessCommand(commands.Split(' '));
		}

		public static async Task<int?> ProcessCommand(string[] commands) {
			try {
				switch(commands[0]) {
				case "getbalance":
					if(commands.Length <= 1)
						Console.WriteLine(await Wallet.GetBalanceAsync());
					else
						Console.WriteLine(await Network.GetAddressBalanceAsync(commands[1]));
					break;
				case "getspendablebalance":
					Console.WriteLine(await Wallet.GetSpendableBalanceAsync());
					break;
				case "getspendableoutputs":
					foreach(Transaction.Output output in await Wallet.GetSpendableOutputsAsync())
						Console.WriteLine("{0}:{1}", output.Transaction.Hash, output.Index);
					break;
				case "getnewaddress":
					Console.WriteLine(await Wallet.GenerateAddressAsync());
					break;

				case "importprivkey":
					string key;
					if(commands.Length <= 1) {
						Console.Write("Enter your private key: ");
						key = ReadHiddenLine();
					}
					else {
						key = commands[1];
					}

					try {
						await Wallet.ImportKeyAsync(key);

						if(commands.Length > 1) {
							Console.WriteLine("Next time, omit the private key when typing a command");
							Console.WriteLine("We'll securely prompt you for it if you leave it blank!");
						}
					}
					catch(FormatException) {
						Console.WriteLine("Invalid private key!");
						Console.WriteLine("Private keys are expected to be in the standard Wallet Import Format");
						Console.WriteLine("These are 51 characters in length, and always begin with the number 5");
					}
					break;
				case "watchaddress":
				case "importwatchaddress":
					await Wallet.ImportWatchAddressAsync(commands[1]);
					break;
				case "importreadonlyaddress":
					await Wallet.ImportReadOnlyAddressAsync(commands[1]);
					break;

				case "listaddresses":
					foreach(Address address in Wallet.Addresses)
						Console.WriteLine(address);
					break;
				case "listprivateaddresses":
				case "listprivaddresses":
				case "listhiddenaddresses":
					foreach(Address address in Wallet.PrivateAddresses)
						Console.WriteLine(address);
					break;
				case "listpublicaddresses":
				case "listpubaddresses":
					foreach(Address address in Wallet.PublicAddresses)
						Console.WriteLine(address);
					break;
				case "listusableaddresses":
				case "listspendableaddresses":
				case "listknownkeyaddresses":
					foreach(Address address in Wallet.PrivateKeys.Keys)
						Console.WriteLine(address);
					break;
				case "listwatchaddresses":
					foreach(Address address in Wallet.WatchAddresses)
						Console.WriteLine(address);
					break;

				case "dumpprivkey":
					if(Wallet.PrivateKeys.ContainsKey(new Address(commands[1])) && Confirm("Are you sure you want to show your private key?"))
						Console.WriteLine(Wallet.PrivateKeys[new Address(commands[1])].ToWalletImportFormat());
					else
						Console.WriteLine("Sorry, you don't have the key to that address in your wallet!");
					break;

				case "publicizeaddress":
				case "publiciseaddress":
				case "showaddress":
				case "unhideaddress":
					await Wallet.ShowAddressAsync(commands[1]);
					break;
				case "privatizeaddress":
				case "privatiseaddress":
				case "hideaddress":
					await Wallet.HideAddressAsync(commands[1]);
					break;

				case "unwatchaddress":
				case "removewatchaddress":
					await Wallet.RemoveWatchAddressAsync(commands[1]);
					break;
				case "removeaddress":
					await Wallet.RemoveAddressAsync(commands[1]);
					break;

				case "sendtoaddress":
					try {
						Dictionary<Address, Money> destinations = new Dictionary<Address, Money> { { new Address(commands[1]), Money.Create(Decimal.Parse(commands[2]), "BTC") } };
						Transaction tx = await Wallet.CreateTransactionAsync(destinations);

						await Network.BroadcastTransactionAsync(tx);
						Console.WriteLine(tx.Hash);

					}
					catch(CoinPicker.InsufficientFundsException) {
						Console.WriteLine("You don't have enough coins");
					}
					break;

				case "getexchangerate":
					Console.WriteLine(await Network.GetExchangeRateAsync(commands[1], "BTC"));
					break;

				case "addmultisigaddress":
				case "backupwallet":
				case "createmultisig":
				case "createrawtransaction":
				case "decoderawtransaction":
				case "listreceivedbyaddress":
				case "listsinceblock":
				case "listtransactions":
				case "move":
				case "sendfrom":
				case "sendmany":
				case "sendrawtransaction":
				case "settxfee":
				case "signmessage":
				case "signrawtransactions":
				case "getblock":
				case "getblockcount":
				case "getblockhash":
				case "getdifficulty":
				case "getinfo":
				case "getmininginfo":
				case "getrawtransaction":
				case "gettransaction":
				case "gettxout":
				case "help":
					throw new NotImplementedException();


				case "lock":
					await Wallet.LockAsync();
					break;
				case "unlock":
					Console.Write("Enter your passphrase: ");
					try {
						await Wallet.UnlockAsync(ReadSecureLine());
					}
					catch(Wallet.InvalidPassphraseException) {
						Console.WriteLine("Invalid passphrase");
					}
					break;
				case "save":
					await Wallet.SaveAsync();
					break;

				case "":
					break;
				case "quit":
				case "exit":
				case "stop":
					if(!Wallet.IsLocked)
						await Wallet.SaveAsync();
					return 0;
				default:
					Console.WriteLine("Unknown command `{0}`", commands[0]);
					break;
				}
			}
			catch(Wallet.LockedException) {
				Console.WriteLine("You need to unlock your wallet first!");
			}
			catch(Wallet.OperationException e) {
				Console.WriteLine(e.Message);
			}

			return null;
		}

		public static bool Confirm(string message = "Are you sure?", string prompt = "[y/n]:") {
			Console.Write("{0} {1} ", message, prompt);
			switch((Console.ReadLine() ?? "").ToLower()) {
			case "y":
			case "yes":
				return true;
			default:
				return false;
			}
		}
		// http://stackoverflow.com/q/3404421/1311454
		public static byte[] ReadSecureLine() {
			List<byte> bytes = new List<byte>();

			ConsoleKeyInfo key = Console.ReadKey(true);
			while(key.Key != ConsoleKey.Enter) {
				if(key.Key == ConsoleKey.Backspace && bytes.Count > 0) {
					bytes.RemoveAt(bytes.Count - 1);
				}
				else {
					bytes.Add((byte)key.KeyChar);
				}

				key = Console.ReadKey(true);
			}

			Console.WriteLine();
			return bytes.ToArray();
		}

		public static string ReadHiddenLine() {
			return new String(ReadSecureLine().Select(b => (char)b).ToArray());
		}
	}
}
