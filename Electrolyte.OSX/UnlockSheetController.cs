using System;
using System.Threading.Tasks;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class UnlockSheetController : NSWindowController {
		[Export("initWithCoder:")]
		public UnlockSheetController(NSCoder coder) : base(coder) { Initialize(); }
		public UnlockSheetController(IntPtr handle) : base(handle) { Initialize(); }
		public UnlockSheetController() : base("UnlockSheet") { Initialize(); }

		public delegate void UnlockEvent(string passphrase);
		public event UnlockEvent OnUnlock = delegate { };
		public event EventHandler OnClose = delegate { };

		void Initialize() { }

		public new UnlockSheet Window { get { return (UnlockSheet)base.Window; } }

		public void Show(NSWindow docWindow) {
			NSApplication.SharedApplication.BeginSheet(Window, docWindow);
		}

		partial void Close(NSObject sender) {
			NSApplication.SharedApplication.EndSheet(Window);
			Window.Close();
			Window.Dispose();

			walletPassphraseField.StringValue = "";

			OnClose(this, new EventArgs());
		}

		partial void Unlock(NSObject sender) {
			OnUnlock(walletPassphraseField.StringValue);
			Close(sender);
		}
	}
}

