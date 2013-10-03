using System;
using System.Threading.Tasks;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public class SheetController : NSWindowController {
		[Export("initWithCoder:")]
		public SheetController(NSCoder coder) : base(coder) { }
		public SheetController(IntPtr handle) : base(handle) { }
		public SheetController(string windowNibName) : base(windowNibName) { }

		public event EventHandler DidOpen = delegate { };
		public event EventHandler DidClose = delegate { };

		public virtual void ShowSheet(NSWindow dockToWindow) {
			NSApplication.SharedApplication.BeginSheet(Window, dockToWindow);
			DidOpen(this, new EventArgs());
		}

		protected void CloseSheet(NSObject sender, EventArgs e) {
			NSApplication.SharedApplication.EndSheet(Window);
			Window.Close();
			Window.Dispose();

			DidClose(this, e);
		}

		public virtual void CloseSheet(NSObject sender = null) {
			CloseSheet(sender, new EventArgs());
		}
	}
}

