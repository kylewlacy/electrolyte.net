using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Electrolyte.Helpers;
using Electrolyte.Messages;
using Electrolyte.Networking;
using Electrolyte.Portable;
using Electrolyte.Standard.IO;

namespace Electrolyte.OSX {
	public partial class WalletWindowController : NSWindowController {
		public new WalletWindow Window {
			get { return (WalletWindow)base.Window; }
		}

		TransactionTableData transactionTableData;
		Wallet wallet;

		UnlockSheetController _unlockSheet;
		UnlockSheetController unlockSheet {
			get {
				if(_unlockSheet == null) {
					_unlockSheet = new UnlockSheetController();
					_unlockSheet.WillUnlock += UnlockWallet;
					_unlockSheet.DidClose += (sender, e) => {
						if((e as UnlockSheetEventArgs ?? new UnlockSheetEventArgs()).WasCancelled)
							LockToggled();
					};
				}

				return _unlockSheet;
			}
		}

		ConfirmTransactionSheetController _confirmTransaction;
		ConfirmTransactionSheetController confirmTransaction {
			get {
				if(_confirmTransaction == null) {
					_confirmTransaction = new ConfirmTransactionSheetController();
				}

				return _confirmTransaction;
			}
		}

		AddressWindowController _addressWindow;
		AddressWindowController AddressWindow {
			get {
				if(_addressWindow == null) {
					_addressWindow = new AddressWindowController(wallet);
				}

				return _addressWindow;
			}
		}
		
		[Export("initWithCoder:")]
		public WalletWindowController(NSCoder coder) : base (coder) { Initialize(); }
		public WalletWindowController(IntPtr handle) : base (handle) { Initialize(); }
		public WalletWindowController() : base ("WalletWindow") { Initialize(); }

		void Initialize() { }

		public override async void WindowDidLoad() {
			sendButton.Enabled = false;

			var walletFile = (FileInfo)Wallet.DefaultWalletFile;
			if(walletFile.Exists)
				wallet = await Wallet.LoadAsync();
			else
				wallet = await CreateWallet(walletFile);

			if(wallet == null)
				NSApplication.SharedApplication.Terminate(this);

			wallet.DidLock += (sender, e) => LockToggled();
			wallet.DidUnlock += (sender, e) => LockToggled();

			transactionTableData = new TransactionTableData();
			transactionTable.DataSource = transactionTableData.DataSource;
			transactionTable.Delegate = transactionTableData.Delegate;

			Task balance = UpdateBalanceAsync();
			Task history = UpdateHistoryAsync();

			LockToggled();

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

			confirmTransaction.ShowSheetWithTransaction(Window, tx);

			confirmTransaction.DidSend += delegate {
				recipientField.StringValue = "";
				valueField.StringValue = "";
			};

			confirmTransaction.DidClose += delegate {
				sendButton.Enabled = true;
			};
		}

		async partial void ToggleLockUnlock(NSObject sender) {
			lockUnlockToggleButton.Enabled = false;
			if(wallet.IsLocked)
				ShowUnlockSheet();
			else
				await wallet.LockAsync();
		}

		partial void ShowAddressWindow(NSObject sender) {
			AddressWindow.ShowWindow(this);
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

		public async Task<Wallet> CreateWallet(FileInfo walletFile) {
			Wallet newWallet = null;
			var creationSheet = new WalletCreationSheetController(walletFile);
			var creationLock = new SemaphoreLite();

			await creationLock.WaitAsync();

			creationSheet.ShowSheet(Window);
			creationSheet.DidCreate += (sender, e) => newWallet = e.Wallet;
			creationSheet.DidClose += (sender, e) => creationLock.Release();

			await creationLock.WaitAsync();
			creationLock.Release();
			return newWallet;
		}
	}
}

