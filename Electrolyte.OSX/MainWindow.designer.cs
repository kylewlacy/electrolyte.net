// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Electrolyte.OSX
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField balanceLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton lockUnlockToggleButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField recipientField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton sendButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView transactionTable { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField valueField { get; set; }

		[Action ("CloseUnlockSheet:")]
		partial void CloseUnlockSheet (MonoMac.Foundation.NSObject sender);

		[Action ("SendTransaction:")]
		partial void SendTransaction (MonoMac.Foundation.NSObject sender);

		[Action ("ToggleLockUnlock:")]
		partial void ToggleLockUnlock (MonoMac.Foundation.NSObject sender);

		[Action ("UnlockWallet:")]
		partial void UnlockWallet (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (balanceLabel != null) {
				balanceLabel.Dispose ();
				balanceLabel = null;
			}

			if (lockUnlockToggleButton != null) {
				lockUnlockToggleButton.Dispose ();
				lockUnlockToggleButton = null;
			}

			if (recipientField != null) {
				recipientField.Dispose ();
				recipientField = null;
			}

			if (sendButton != null) {
				sendButton.Dispose ();
				sendButton = null;
			}

			if (transactionTable != null) {
				transactionTable.Dispose ();
				transactionTable = null;
			}

			if (valueField != null) {
				valueField.Dispose ();
				valueField = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		MonoMac.AppKit.NSTextField balanceLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton sendButton { get; set; }

		[Action ("SendClick:")]
		partial void SendClick (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (balanceLabel != null) {
				balanceLabel.Dispose ();
				balanceLabel = null;
			}

			if (sendButton != null) {
				sendButton.Dispose ();
				sendButton = null;
			}
		}
	}
}
