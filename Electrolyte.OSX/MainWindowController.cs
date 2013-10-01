using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Electrolyte.Helpers;
using Electrolyte.Messages;
using Electrolyte.Networking;
using Electrolyte.Standard.IO;

namespace Electrolyte.OSX {
	public partial class MainWindowController : NSWindowController {
		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}

		TransactionTableData transactionTableData;
		Wallet wallet;

		UnlockSheetController _unlockSheet;
		UnlockSheetController unlockSheet {
			get {
				if(_unlockSheet == null) {
					_unlockSheet = new UnlockSheetController();
					_unlockSheet.OnUnlock += UnlockWallet;
					_unlockSheet.OnClose += (sender, e) => {
						if((e as UnlockSheetEventArgs ?? new UnlockSheetEventArgs()).WasCancelled)
							LockToggled();
					};
				}

				return _unlockSheet;
			}
		}
		
		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base (coder) { Initialize(); }
		public MainWindowController(IntPtr handle) : base (handle) { Initialize(); }
		public MainWindowController() : base ("MainWindow") { Initialize(); }

		void Initialize() { }

		public override async void AwakeFromNib() {
			Window.ContentMinSize = new SizeF(530, 240);

			sendButton.Enabled = false;

			FileInfo walletInfo = (FileInfo)Wallet.DefaultWalletPath;
			wallet = await Wallet.LoadAsync(walletInfo);

			wallet.DidLock += (sender, e) => LockToggled();
			wallet.DidUnlock += (sender, e) => LockToggled();

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
			balanceLabel.StringValue = (await wallet.GetCachedBalanceAsync()).ToString();

			balanceLabel.StringValue = (await wallet.GetBalanceAsync()).ToString();
			balanceLabel.TextColor = NSColor.ControlText;
		}

		public async Task UpdateHistoryAsync() {
			List<Transaction.Delta> deltas = (await wallet.GetTransactionDeltasAsync());
			deltas.Sort((a, b) => a.Transaction.Height.Value.CompareTo(b.Transaction.Height.Value));

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

		public void LockToggled() {
			InvokeOnMainThread(() => LockToggled(null, new EventArgs()));
		}

		public void LockToggled(object sender, EventArgs e) {
			sendButton.Enabled = !wallet.IsLocked;

			lockUnlockToggleButton.Enabled = true;
			lockUnlockToggleButton.Title = wallet.IsLocked ? "Unlock" : "Lock";
		}

		public void ShowUnlockSheet() {
			unlockSheet.ShowSheet(Window);
		}

		public async void UnlockWallet(string passphrase) {
			await wallet.UnlockAsync(passphrase);
		}
	}
}

