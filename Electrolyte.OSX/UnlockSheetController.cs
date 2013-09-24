using System;
using System.Threading.Tasks;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public class UnlockSheetEventArgs : EventArgs {
		public bool WasCancelled;

		public UnlockSheetEventArgs(bool wasCancelled = false) : base() {
			WasCancelled = wasCancelled;
		}
	}

	public partial class UnlockSheetController : SheetController {
		[Export("initWithCoder:")]
		public UnlockSheetController(NSCoder coder) : base(coder) { Initialize(); }
		public UnlockSheetController(IntPtr handle) : base(handle) { Initialize(); }
		public UnlockSheetController() : base("UnlockSheet") { Initialize(); }

		public delegate void UnlockEvent(string passphrase);
		public event UnlockEvent OnUnlock = delegate { };

		void Initialize() { }

		public new UnlockSheet Window { get { return (UnlockSheet)base.Window; } }

		public override void CloseSheet(NSObject sender = null) {
			base.CloseSheet(sender);
			walletPassphraseField.StringValue = "";
		}

		partial void Unlock(NSObject sender) {
			OnUnlock(walletPassphraseField.StringValue);
			CloseSheet();
		}

		partial void Cancel(NSObject sender) {
			CloseSheet(sender, new UnlockSheetEventArgs(wasCancelled: true));
		}
	}
}

