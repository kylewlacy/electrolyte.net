using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Electrolyte.Messages;
using Electrolyte.Networking;

namespace Electrolyte.OSX {
	public partial class MainWindowController : NSWindowController {
		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}

		TransactionTableData transactionTableData;
		Wallet wallet;
		
		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base (coder) { Initialize(); }
		public MainWindowController(IntPtr handle) : base (handle) { Initialize(); }
		public MainWindowController() : base ("MainWindow") { Initialize(); }

		void Initialize() { }

		public override async void AwakeFromNib() {
			Window.ContentMinSize = new SizeF(530, 240);

			sendButton.Enabled = false;

			wallet = await Wallet.LoadAsync();
			wallet.DidLock += LockToggled;
			wallet.DidUnlock += LockToggled;

			transactionTableData = new TransactionTableData();
			transactionTable.DataSource = transactionTableData.DataSource;
			transactionTable.Delegate = transactionTableData.Delegate;

			Task balance = UpdateBalanceAsync();
			Task history = UpdateHistoryAsync();

			await balance;
			await history;
		}

		public async Task UpdateBalanceAsync() {
			balanceLabel.TextColor = NSColor.DisabledControlText;
			balanceLabel.StringValue = (await wallet.GetBalanceAsync()).ToString();
			balanceLabel.TextColor = NSColor.ControlText;
		}

		public async Task UpdateHistoryAsync() {
			List<KeyValuePair<Transaction, Money>> deltas = (await wallet.GetTransactionDeltasAsync()).ToList();
			deltas.Sort((first, next) => { return first.Key.Height.Value.CompareTo(next.Key.Height.Value); });

			transactionTableData.TransactionDeltas = deltas;
			transactionTable.ReloadData();
		}

		async partial void SendTransaction(NSObject sender) {
			Address recipient = new Address(recipientField.StringValue);
			Money value = Money.Create(Decimal.Parse(valueField.StringValue), "BTC");
			
			Dictionary<Address, Money> destinations = new Dictionary<Address, Money> { { recipient, value } };

			sendButton.Enabled = false;

			Transaction tx = await wallet.CreateTransactionAsync(destinations);
			await Network.BroadcastTransactionAsync(tx);

			sendButton.Enabled = true;
		}

		async partial void ToggleLockUnlock(NSObject sender) {
			lockUnlockToggleButton.Enabled = false;
			if(wallet.IsLocked)
				ShowUnlockSheet();
			else
				await wallet.LockAsync();
		}

		public void LockToggled(object sender = null, EventArgs e = null) {
			sendButton.Enabled = !wallet.IsLocked;

			lockUnlockToggleButton.Enabled = true;
			lockUnlockToggleButton.Title = wallet.IsLocked ? "Unlock" : "Lock";
		}

		public void ShowUnlockSheet() {
			if(unlockSheet == null)
				NSBundle.LoadNib("UnlockSheet", this);

			NSApplication.SharedApplication.BeginSheet(unlockSheet, Window);
		}

		partial void CloseUnlockSheet(NSObject sender) {
			NSApplication.SharedApplication.EndSheet(unlockSheet);
			unlockSheet.Close();
			unlockSheet.Dispose();
			unlockSheet = null;
		}

		async partial void UnlockWallet(NSObject sender) {
			string passphrase = walletPassphraseField.StringValue;
			NSButton button = sender as NSButton;

			Task unlockWallet = wallet.UnlockAsync(passphrase);
			CloseUnlockSheet(sender);
			await unlockWallet;
		}
	}
}

