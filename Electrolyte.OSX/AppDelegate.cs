using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace Electrolyte.OSX {
	public partial class AppDelegate : NSApplicationDelegate {
		WalletWindowController walletWindowController;

		public override void FinishedLaunching(NSObject notification) {
			walletWindowController = new WalletWindowController();
			walletWindowController.Window.MakeKeyAndOrderFront(this);
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) {
			return true;
		}
	}
}

