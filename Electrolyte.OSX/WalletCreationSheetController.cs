using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Electrolyte.Standard.IO;

namespace Electrolyte.OSX {
	public class WalletCreationEventArgs : EventArgs {
		public Wallet Wallet;

		public WalletCreationEventArgs(Wallet wallet) : base() {
			Wallet = wallet;
		}
	}

	public partial class WalletCreationSheetController : SheetController {
		public Electrolyte.Portable.IO.FileInfo WalletFile { get; set; }
		public delegate void WalletCreationEvent(object sender, WalletCreationEventArgs e);
		public event WalletCreationEvent DidCreate = delegate { };

		public new WalletCreationSheet Window {
			get { return (WalletCreationSheet)base.Window; }
		}

		[Export("initWithCoder:")]
		public WalletCreationSheetController(NSCoder coder) : base(coder) { Initialize(); }
		public WalletCreationSheetController(IntPtr handle) : base(handle) { Initialize(); }
		public WalletCreationSheetController() : base("WalletCreationSheet") { Initialize(); }
		public WalletCreationSheetController(FileInfo walletFile) : base("WalletCreationSheet") { Initialize(walletFile); }

		void Initialize() { Initialize((FileInfo)Wallet.DefaultWalletPath); }
		void Initialize(FileInfo walletFile) {
			WalletFile = walletFile;
		}

		void PassphraseChanged(object sender, EventArgs e) {
			if(passphraseField.StringValue.Length > 0)
				createButton.Enabled = true;
			else
				createButton.Enabled = false;
		}

		partial void Cancel(NSObject sender) {
			CloseSheet();
		}

		async partial void Create(NSObject sender) {
			Wallet wallet = await Wallet.CreateAsync(passphraseField.StringValue, WalletFile);
			await wallet.SaveAsync();

			DidCreate(this, new WalletCreationEventArgs(wallet));
			CloseSheet();
		}

		public override void ShowSheet(NSWindow dockToWindow) {
			base.ShowSheet(dockToWindow);
			passphraseField.Changed += PassphraseChanged;
		}

		public override void CloseSheet(NSObject sender = null) {
			passphraseField.Changed -= PassphraseChanged;
			base.CloseSheet(sender);
		}
	}
}

