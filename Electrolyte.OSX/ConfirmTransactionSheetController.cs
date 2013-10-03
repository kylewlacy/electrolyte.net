using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Electrolyte.Messages;
using Electrolyte.Networking;

namespace Electrolyte.OSX {
	public partial class ConfirmTransactionSheetController : SheetController {
		[Export("initWithCoder:")]
		public ConfirmTransactionSheetController(NSCoder coder) : base(coder) { Initialize(); }
		public ConfirmTransactionSheetController(IntPtr handle) : base(handle) { Initialize(); }
		public ConfirmTransactionSheetController() : base("ConfirmTransactionSheet") { Initialize(); }

		public Transaction Transaction { get; private set; }
		public EventHandler DidSend = delegate { };
		public EventHandler DidDeny = delegate { };

		void Initialize() { }

		public new ConfirmTransactionSheet Window {
			get { return (ConfirmTransactionSheet)base.Window; }
		}

		public void ShowSheetWithTransaction(NSWindow dockToWindow, Transaction transaction) {
			Transaction = transaction;
			ShowSheet(dockToWindow);
		}

		async partial void Send(NSObject sender) {
			var button = sender as NSButton;
			if(button != null)
				button.Enabled = false;

			await Network.BroadcastTransactionAsync(Transaction);
			DidSend(this, new EventArgs());
			CloseSheet();
		}

		partial void Deny(NSObject sender) {
			Transaction = null;
			DidDeny(this, new EventArgs());
			CloseSheet(this);
		}
	}
}

