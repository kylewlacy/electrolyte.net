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

		public event EventHandler OnOpen = delegate { };
		public event EventHandler OnClose = delegate { };

		public virtual void ShowSheet(NSWindow dockToWindow) {
			NSApplication.SharedApplication.BeginSheet(Window, dockToWindow);
			OnOpen(this, new EventArgs());
		}

		public virtual void CloseSheet(NSObject sender = null) {
			NSApplication.SharedApplication.EndSheet(Window);
			Window.Close();
			Window.Dispose();

			OnClose(this, new EventArgs());
		}
	}
}

